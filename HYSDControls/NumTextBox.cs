using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HYSDControls // 请改成你自己的命名空间
{
    public class NumPadValueChangedEventArgs : EventArgs
    {
        public string TextValue { get; }
        public decimal? DecimalValue { get; }

        public NumPadValueChangedEventArgs(string textValue)
        {
            TextValue = textValue;
            if (decimal.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
            {
                DecimalValue = v;
            }
            else
            {
                DecimalValue = null;
            }
        }
    }

    [ToolboxBitmap(typeof(TextBox))]
    public class NumTextBox : TextBox
    {
        private NumPadForm _currentPad;

        [Category("数字键盘")]
        [Description("当弹出键盘上点击OK时触发，携带最终确认的数值")]
        public event EventHandler<NumPadValueChangedEventArgs> NumPadOkPressed;

        [Category("数字键盘")]
        [Description("键盘输入过程中实时触发，携带当前显示的数值")]
        public event EventHandler<NumPadValueChangedEventArgs> NumPadValueChanged;

        [Category("数字键盘")]
        [Description("是否启用点击弹出数字键盘")]
        public bool UseNumPad { get; set; } = false;

        [Category("数字键盘")]
        [Description("允许输入的最大值")]
        public decimal? NumMaxValue { get; set; }

        [Category("数字键盘")]
        [Description("允许输入的最小值")]
        public decimal? NumMinValue { get; set; }

        [Category("数字键盘")]
        [Description("是否允许输入小数")]
        public bool NumAllowDecimal { get; set; } = true;

        // 当前是否正在通过数字键盘编辑数据
        [Browsable(false)]
        public bool IsEditing { get; internal set; } = false;

        // 【新增】写入PLC后期望读回来的值，用于防止旧值覆盖新值
        [Browsable(false)]
        public string ExpectedValue { get; internal set; } = null;

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (UseNumPad)
            {
                this.SelectionStart = this.Text.Length;
                this.SelectionLength = 0;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (UseNumPad)
            {
                if (_currentPad != null && !_currentPad.IsDisposed)
                {
                    _currentPad.Close();
                }

                var newPad = new NumPadForm(this)
                {
                    MaxValue = this.NumMaxValue,
                    MinValue = this.NumMinValue,
                    AllowDecimal = this.NumAllowDecimal
                };

                newPad.OkClicked += (args) => this.NumPadOkPressed?.Invoke(this, args);
                newPad.ValueChanged += (args) => this.NumPadValueChanged?.Invoke(this, args);

                _currentPad = newPad;
                this.IsEditing = true;

                this.BeginInvoke(new Action(() =>
                {
                    if (object.ReferenceEquals(_currentPad, newPad) && !newPad.IsDisposed)
                    {
                        newPad.ShowBelow(this);
                    }
                    else
                    {
                        if (!newPad.IsDisposed)
                        {
                            newPad.Dispose();
                        }
                    }
                }));
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!UseNumPad) { base.OnKeyPress(e); return; }
            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (NumAllowDecimal && e.KeyChar == '.' && !this.Text.Contains(".")) return;
            e.Handled = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && UseNumPad)
                return;
            base.OnMouseDown(e);
        }

        // 【新增】提供给外部定时器刷新PLC数据的专用方法
        public void UpdateValueFromPlc(string plcValue)
        {
            // 1. 如果正在用键盘编辑，拒绝刷新
            if (IsEditing) return;

            // 2. 如果存在期望值（刚刚通过键盘写入过）
            if (!string.IsNullOrEmpty(ExpectedValue))
            {
                // 如果PLC读回来的值和期望值一致，说明PLC已经同步完成
                if (plcValue == ExpectedValue)
                {
                    ExpectedValue = null; // 清除期望值，恢复正常刷新
                    this.Text = plcValue;
                }
                else
                {
                    // 如果不一致，说明PLC还没处理完，读回来的还是旧值
                    // 什么都不做，保持界面当前显示的新值，防止被旧值覆盖
                }
            }
            else
            {
                // 3. 正常实时刷新
                this.Text = plcValue;
            }
        }
    }

    public class NumPadForm : Form
    {
        public event Action<NumPadValueChangedEventArgs> OkClicked;
        public event Action<NumPadValueChangedEventArgs> ValueChanged;

        private NumTextBox _targetTextBox;
        private bool _allowDecimal = true;
        private string _currentValue = "";
        private string _originalValue = "";
        private bool _isFirstInput = true;
        private bool isFlashing = false;
        private bool isDisposed = false;
        private bool _isCommitting = false;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x02;

        public bool AllowDecimal { get => _allowDecimal; set => _allowDecimal = value; }

        private decimal? _maxValue;
        public decimal? MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; UpdateRangeDisplay(); }
        }

        private decimal? _minValue;
        public decimal? MinValue
        {
            get => _minValue;
            set { _minValue = value; UpdateRangeDisplay(); }
        }

        private Panel headerPanel;
        private Panel displayPanel;
        private TextBox txtDisplay;
        private Label lblRange;

        private System.Windows.Forms.Timer flashTimer;
        private int flashState = 0;
        private Action flashCallback;
        private Color originalPanelColor;
        private Color originalTextColor;
        private Color alertPanelColor = Color.FromArgb(220, 20, 20);
        private Color alertTextColor = Color.White;

        public NumPadForm(NumTextBox targetTextBox)
        {
            _targetTextBox = targetTextBox ?? throw new ArgumentNullException(nameof(targetTextBox));
            _currentValue = _targetTextBox.Text;
            _originalValue = _targetTextBox.Text;
            _isFirstInput = true;
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x00000080;
                return cp;
            }
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(220, 290);
            this.Deactivate += (s, e) => this.Close();
            this.Shown += (s, e) => UpdateDisplaySelection();

            headerPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(230, 230, 230)
            };

            Button btnClose = new Button
            {
                Text = "X",
                Dock = DockStyle.Right,
                Width = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Gray,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 100);
            btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 50, 50);
            btnClose.Click += (s, e) => this.Close();

            Label lblTitle = new Label
            {
                Text = "Input Content",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.Gray,
                Padding = new Padding(5, 0, 0, 0),
                Cursor = Cursors.SizeAll
            };

            headerPanel.MouseDown += HeaderPanel_MouseDown;
            lblTitle.MouseDown += HeaderPanel_MouseDown;

            headerPanel.Controls.Add(btnClose);
            headerPanel.Controls.Add(lblTitle);

            displayPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(5)
            };

            lblRange = new Label
            {
                Text = "无限制",
                Dock = DockStyle.Bottom,
                Height = 15,
                TextAlign = ContentAlignment.BottomLeft,
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent
            };

            txtDisplay = new TextBox
            {
                Text = "",
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Consolas", 24F, FontStyle.Bold),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(245, 245, 245),
                ReadOnly = false,
                ShortcutsEnabled = false,
                Cursor = Cursors.Default
            };

            txtDisplay.KeyDown += (s, e) => e.Handled = true;
            txtDisplay.KeyPress += (s, e) => e.Handled = true;
            txtDisplay.Enter += (s, e) => UpdateDisplaySelection();
            txtDisplay.Click += (s, e) => UpdateDisplaySelection();

            displayPanel.Controls.Add(lblRange);
            displayPanel.Controls.Add(txtDisplay);

            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                BackColor = Color.WhiteSmoke
            };

            for (int i = 0; i < 4; i++) tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            for (int i = 0; i < 4; i++) tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            string[,] btnTexts = {
                { "7", "8", "9", "<-" },
                { "4", "5", "6", "C"  },
                { "1", "2", "3", "OK" },
                { "0", "0", "0", "."  }
            };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == 3 && j < 2) continue;

                    var btn = new Button
                    {
                        Text = btnTexts[i, j],
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2),
                        Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Tag = btnTexts[i, j]
                    };

                    if (btnTexts[i, j] == "OK")
                    {
                        btn.BackColor = Color.LightGreen;
                        btn.ForeColor = Color.DarkGreen;
                    }
                    else if (btnTexts[i, j] == "<-")
                    {
                        btn.BackColor = Color.FromArgb(255, 179, 71);
                        btn.ForeColor = Color.White;
                    }
                    else if (btnTexts[i, j] == "C")
                    {
                        btn.BackColor = Color.FromArgb(255, 105, 97);
                        btn.ForeColor = Color.White;
                    }

                    btn.Click += Btn_Click;

                    if (i == 3 && j == 0)
                    {
                        tlp.Controls.Add(btn, 0, 3);
                        tlp.SetColumnSpan(btn, 3);
                    }
                    else
                    {
                        tlp.Controls.Add(btn, j, i);
                    }
                }
            }

            this.Controls.Add(tlp);
            this.Controls.Add(displayPanel);
            this.Controls.Add(headerPanel);

            originalPanelColor = displayPanel.BackColor;
            originalTextColor = txtDisplay.ForeColor;

            flashTimer = new System.Windows.Forms.Timer();
            flashTimer.Interval = 100;
            flashTimer.Tick += FlashTimer_Tick;

            UpdateRangeDisplay();

            txtDisplay.Text = _currentValue;
        }

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            if (_targetTextBox != null && !_targetTextBox.IsDisposed)
            {
                _targetTextBox.IsEditing = false;

                if (!_isCommitting)
                {
                    _targetTextBox.SelectionLength = 0;

                    Form parentForm = _targetTextBox.FindForm();
                    if (parentForm != null && !parentForm.IsDisposed)
                    {
                        parentForm.ActiveControl = null;
                    }
                }
            }
        }

        private void UpdateDisplaySelection()
        {
            this.BeginInvoke(new Action(() =>
            {
                if (isDisposed) return;

                if (_isFirstInput && _currentValue.Length > 0)
                {
                    txtDisplay.SelectionStart = 0;
                    txtDisplay.SelectionLength = _currentValue.Length;
                }
                else if (_currentValue.Length > 0)
                {
                    txtDisplay.SelectionStart = _currentValue.Length - 1;
                    txtDisplay.SelectionLength = 1;
                }
                else
                {
                    txtDisplay.SelectionStart = 0;
                    txtDisplay.SelectionLength = 0;
                }

                if (!txtDisplay.Focused) txtDisplay.Focus();
            }));
        }

        private void UpdateDisplay()
        {
            txtDisplay.Text = _currentValue;
            UpdateDisplaySelection();
            ValueChanged?.Invoke(new NumPadValueChangedEventArgs(_currentValue));
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (isFlashing) return;

            if (sender is Button btn)
            {
                string key = btn.Tag.ToString();
                switch (key)
                {
                    case "<-":
                        _isFirstInput = false;
                        if (_currentValue.Length > 0)
                            _currentValue = _currentValue.Substring(0, _currentValue.Length - 1);
                        UpdateDisplay();
                        break;

                    case "C":
                        _isFirstInput = false;
                        _currentValue = "";
                        UpdateDisplay();
                        break;

                    case "OK":
                        HandleOk();
                        break;

                    case ".":
                        _isFirstInput = false;
                        if (_allowDecimal && !_currentValue.Contains("."))
                        {
                            if (string.IsNullOrEmpty(_currentValue))
                            {
                                _currentValue = "0.";
                            }
                            else
                            {
                                _currentValue += ".";
                            }
                            UpdateDisplay();
                        }
                        break;

                    default:
                        HandleNumberInput(key);
                        break;
                }
            }
        }

        private void HandleNumberInput(string key)
        {
            string proposedText;

            if (_isFirstInput)
            {
                proposedText = key;
                _isFirstInput = false;
            }
            else
            {
                proposedText = _currentValue + key;
            }

            if (MaxValue.HasValue)
            {
                if (decimal.TryParse(proposedText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                {
                    if (val > MaxValue.Value)
                    {
                        if (_isFirstInput == false && proposedText == key) _isFirstInput = true;
                        StartFlash(() => { });
                        return;
                    }
                }
            }

            _currentValue = proposedText;
            UpdateDisplay();
        }

        private void HandleOk()
        {
            string txt = _currentValue.Trim();
            decimal? currentVal = null;
            if (decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                currentVal = v;

            bool needFlash = false;
            if (!currentVal.HasValue && MinValue.HasValue) needFlash = true;
            else if (currentVal.HasValue && MinValue.HasValue && currentVal < MinValue) needFlash = true;
            else if (currentVal.HasValue && MaxValue.HasValue && currentVal > MaxValue) needFlash = true;

            if (needFlash)
            {
                StartFlash(() =>
                {
                    if (!isDisposed)
                    {
                        if (!currentVal.HasValue || currentVal < MinValue)
                            _currentValue = MinValue.Value.ToString();
                        else if (currentVal > MaxValue)
                            _currentValue = MaxValue.Value.ToString();

                        UpdateDisplay();
                        CommitToTarget();
                    }
                });
            }
            else
            {
                CommitToTarget();
            }
        }

        private void CommitToTarget()
        {
            _isCommitting = true;

            _targetTextBox.Text = _currentValue;

            // 【新增】设置期望值，防止PLC读回旧值时覆盖界面
            _targetTextBox.ExpectedValue = _currentValue;

            _targetTextBox.SelectionLength = 0;
            Form parentForm = _targetTextBox.FindForm();
            if (parentForm != null && !parentForm.IsDisposed)
            {
                parentForm.ActiveControl = null;
            }

            OkClicked?.Invoke(new NumPadValueChangedEventArgs(_currentValue));
            this.Close();
        }

        private void StartFlash(Action callback)
        {
            isFlashing = true;
            flashCallback = callback;
            flashState = 0;
            flashTimer.Start();
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            if (isDisposed)
            {
                flashTimer.Stop();
                return;
            }

            flashState++;

            if (flashState <= 6)
            {
                if (flashState % 2 == 1)
                {
                    displayPanel.BackColor = alertPanelColor;
                    txtDisplay.BackColor = alertPanelColor;
                    txtDisplay.ForeColor = alertTextColor;
                    lblRange.ForeColor = alertTextColor;
                }
                else
                {
                    displayPanel.BackColor = originalPanelColor;
                    txtDisplay.BackColor = originalPanelColor;
                    txtDisplay.ForeColor = originalTextColor;
                    lblRange.ForeColor = Color.Gray;
                }
                displayPanel.Invalidate(true);
            }
            else
            {
                flashTimer.Stop();
                displayPanel.BackColor = originalPanelColor;
                txtDisplay.BackColor = originalPanelColor;
                txtDisplay.ForeColor = originalTextColor;
                lblRange.ForeColor = Color.Gray;
                displayPanel.Invalidate(true);

                isFlashing = false;
                flashCallback?.Invoke();
            }
        }

        private void UpdateRangeDisplay()
        {
            if (MinValue.HasValue && MaxValue.HasValue)
                lblRange.Text = $"范围: {MinValue.Value} ~ {MaxValue.Value}";
            else if (MinValue.HasValue)
                lblRange.Text = $"最小: {MinValue.Value}";
            else if (MaxValue.HasValue)
                lblRange.Text = $"最大: {MaxValue.Value}";
            else
                lblRange.Text = "无限制";
        }

        public void ShowBelow(Control control)
        {
            Point screenPoint = control.PointToScreen(new Point(0, control.Height));

            Screen currentScreen = Screen.FromControl(control);
            if (screenPoint.Y + this.Height > currentScreen.WorkingArea.Height)
            {
                screenPoint.Y = control.PointToScreen(Point.Empty).Y - this.Height;
            }
            if (screenPoint.X + this.Width > currentScreen.WorkingArea.Width)
            {
                screenPoint.X = currentScreen.WorkingArea.Right - this.Width;
            }

            this.Location = screenPoint;
            this.Show();
        }

        protected override void Dispose(bool disposing)
        {
            isDisposed = true;
            if (flashTimer != null) flashTimer.Stop();
            base.Dispose(disposing);
        }
    }
}
