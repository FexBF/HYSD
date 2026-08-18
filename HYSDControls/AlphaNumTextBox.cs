using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HYSDControls
{
    [ToolboxBitmap(typeof(TextBox))]
    public class AlphaNumTextBox : TextBox
    {
        private AlphaNumPadForm _currentPad;

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        [Category("字母数字键盘")]
        [Description("当弹出键盘上点击OK时触发")]
        public event EventHandler AlphaNumPadOkPressed;

        [Category("字母数字键盘")]
        [Description("是否启用点击弹出字母数字键盘")]
        public bool UseAlphaNumPad { get; set; } = false;

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (UseAlphaNumPad)
            {
                HideCaret(this.Handle);
                this.SelectionStart = this.Text.Length;
                this.SelectionLength = 0;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (UseAlphaNumPad)
            {
                this.SelectionStart = this.Text.Length;
                this.SelectionLength = 0;

                if (_currentPad != null && !_currentPad.IsDisposed)
                {
                    _currentPad.Close();
                }

                var newPad = new AlphaNumPadForm(this);
                newPad.OkClicked += () => this.AlphaNumPadOkPressed?.Invoke(this, EventArgs.Empty);
                _currentPad = newPad;

                this.BeginInvoke(new Action(() =>
                {
                    if (object.ReferenceEquals(_currentPad, newPad) && !newPad.IsDisposed)
                    {
                        newPad.ShowBelow(this);
                    }
                    else
                    {
                        if (!newPad.IsDisposed) newPad.Dispose();
                    }
                }));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && UseAlphaNumPad) return;
            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (UseAlphaNumPad)
            {
                if (e.Control && (e.KeyCode == Keys.A || e.KeyCode == Keys.C || e.KeyCode == Keys.X || e.KeyCode == Keys.V))
                    e.SuppressKeyPress = true;
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
                    e.SuppressKeyPress = true;
            }
            base.OnKeyDown(e);
        }
    }


    public class AlphaNumPadForm : Form
    {
        public event Action OkClicked;
        private TextBox _targetTextBox;

        private string _currentInput;
        private bool _isFirstInput = true;

        private bool _isShifted = false;
        private bool _isSymbolMode = false;

        // 【修改】使用不可点击的自定义TextBox
        private NonClickableTextBox txtDisplay;
        private Panel topPanel;
        private Panel headerPanel;
        private Panel keyPanel;
        private Button btnShift;

        private Color btnDefaultColor = Color.White;
        private Color btnShiftColor = Color.FromArgb(173, 216, 230);
        private Color btnDeleteColor = Color.FromArgb(255, 179, 71);
        private Color btnClearColor = Color.FromArgb(255, 105, 97);
        private Color btnOkColor = Color.FromArgb(144, 238, 144);
        private Color btnToggleColor = Color.FromArgb(225, 225, 225);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x02;

        public AlphaNumPadForm(TextBox targetTextBox)
        {
            _targetTextBox = targetTextBox ?? throw new ArgumentNullException(nameof(targetTextBox));
            _currentInput = _targetTextBox.Text;
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
            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(560, 310);

            this.Deactivate += (s, e) => this.Close();

            headerPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            headerPanel.MouseDown += HeaderPanel_MouseDown;

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
            lblTitle.MouseDown += HeaderPanel_MouseDown;

            headerPanel.Controls.Add(btnClose);
            headerPanel.Controls.Add(lblTitle);


            topPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(5, 0, 5, 0)
            };

            // 【修改】实例化不可点击的自定义TextBox
            txtDisplay = new NonClickableTextBox
            {
                Text = _currentInput,
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Right,
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                HideSelection = false,
                Cursor = Cursors.Default
            };

            topPanel.Controls.Add(txtDisplay);

            keyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            GenerateKeys();

            this.Controls.Add(keyPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(headerPanel);
        }

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void GenerateKeys()
        {
            keyPanel.Controls.Clear();
            btnShift = null;

            var keyRows = new List<List<KeyValuePair<string, float>>>()
            {
                new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("1", 1), new KeyValuePair<string, float>("2", 1),
                    new KeyValuePair<string, float>("3", 1), new KeyValuePair<string, float>("4", 1),
                    new KeyValuePair<string, float>("5", 1), new KeyValuePair<string, float>("6", 1),
                    new KeyValuePair<string, float>("7", 1), new KeyValuePair<string, float>("8", 1),
                    new KeyValuePair<string, float>("9", 1), new KeyValuePair<string, float>("0", 1),
                    new KeyValuePair<string, float>("<-", 1.5f)
                }
            };

            if (!_isSymbolMode)
            {
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("Q", 1), new KeyValuePair<string, float>("W", 1),
                    new KeyValuePair<string, float>("E", 1), new KeyValuePair<string, float>("R", 1),
                    new KeyValuePair<string, float>("T", 1), new KeyValuePair<string, float>("Y", 1),
                    new KeyValuePair<string, float>("U", 1), new KeyValuePair<string, float>("I", 1),
                    new KeyValuePair<string, float>("O", 1), new KeyValuePair<string, float>("P", 1)
                });
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("A", 1), new KeyValuePair<string, float>("S", 1),
                    new KeyValuePair<string, float>("D", 1), new KeyValuePair<string, float>("F", 1),
                    new KeyValuePair<string, float>("G", 1), new KeyValuePair<string, float>("H", 1),
                    new KeyValuePair<string, float>("J", 1), new KeyValuePair<string, float>("K", 1),
                    new KeyValuePair<string, float>("L", 1), new KeyValuePair<string, float>("OK", 1.5f)
                });
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("Shift", 1.5f),
                    new KeyValuePair<string, float>("Z", 1), new KeyValuePair<string, float>("X", 1),
                    new KeyValuePair<string, float>("C", 1), new KeyValuePair<string, float>("V", 1),
                    new KeyValuePair<string, float>("B", 1), new KeyValuePair<string, float>("N", 1),
                    new KeyValuePair<string, float>("M", 1), new KeyValuePair<string, float>(".", 1),
                    new KeyValuePair<string, float>("-", 1)
                });
            }
            else
            {
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("!", 1), new KeyValuePair<string, float>("@", 1),
                    new KeyValuePair<string, float>("#", 1), new KeyValuePair<string, float>("$", 1),
                    new KeyValuePair<string, float>("%", 1), new KeyValuePair<string, float>("^", 1),
                    new KeyValuePair<string, float>("&", 1), new KeyValuePair<string, float>("*", 1),
                    new KeyValuePair<string, float>("(", 1), new KeyValuePair<string, float>(")", 1)
                });
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("-", 1), new KeyValuePair<string, float>("_", 1),
                    new KeyValuePair<string, float>("=", 1), new KeyValuePair<string, float>("+", 1),
                    new KeyValuePair<string, float>("{", 1), new KeyValuePair<string, float>("}", 1),
                    new KeyValuePair<string, float>("[", 1), new KeyValuePair<string, float>("]", 1),
                    new KeyValuePair<string, float>("|", 1), new KeyValuePair<string, float>("OK", 1.5f)
                });
                keyRows.Add(new List<KeyValuePair<string, float>>
                {
                    new KeyValuePair<string, float>("ABC", 1.5f),
                    new KeyValuePair<string, float>("\\", 1),
                    new KeyValuePair<string, float>(":", 1), new KeyValuePair<string, float>(";", 1),
                    new KeyValuePair<string, float>("\"", 1), new KeyValuePair<string, float>("'", 1),
                    new KeyValuePair<string, float>("<", 1), new KeyValuePair<string, float>(">", 1),
                    new KeyValuePair<string, float>("?", 1), new KeyValuePair<string, float>("/", 1)
                });
            }

            keyRows.Add(new List<KeyValuePair<string, float>>
            {
                new KeyValuePair<string, float>("CLR", 1.5f),
                new KeyValuePair<string, float>("Space", 6f),
                new KeyValuePair<string, float>(_isSymbolMode ? "ABC" : "#+=", 1.5f)
            });

            int padding = 3;
            int y = padding;

            foreach (var row in keyRows)
            {
                float totalRatio = 0;
                foreach (var key in row) totalRatio += key.Value;

                int x = padding;
                int rowHeight = 42;

                foreach (var keyPair in row)
                {
                    string keyText = keyPair.Key;
                    int btnWidth = (int)((this.ClientSize.Width - padding * 2) * (keyPair.Value / totalRatio)) - padding;

                    var btn = new Button
                    {
                        Text = keyText,
                        Location = new Point(x, y),
                        Size = new Size(btnWidth, rowHeight),
                        Font = new Font("微软雅黑", 12F, FontStyle.Bold),
                        FlatStyle = FlatStyle.Flat,
                        Tag = keyText
                    };

                    switch (keyText)
                    {
                        case "Shift":
                            btn.BackColor = _isShifted ? btnShiftColor : btnDefaultColor;
                            btn.ForeColor = Color.Black;
                            btn.Tag = "Shift";
                            btnShift = btn;
                            break;
                        case "ABC":
                        case "#+=":
                            btn.BackColor = btnToggleColor;
                            btn.ForeColor = Color.Black;
                            btn.Tag = "ToggleSymbol";
                            break;
                        case "<-":
                            btn.BackColor = btnDeleteColor;
                            btn.ForeColor = Color.White;
                            break;
                        case "CLR":
                            btn.BackColor = btnClearColor;
                            btn.ForeColor = Color.White;
                            break;
                        case "OK":
                            btn.BackColor = btnOkColor;
                            btn.ForeColor = Color.DarkGreen;
                            break;
                        case "Space":
                            btn.Font = new Font("微软雅黑", 10F);
                            btn.BackColor = btnDefaultColor;
                            break;
                        default:
                            btn.BackColor = btnDefaultColor;
                            btn.ForeColor = Color.Black;
                            break;
                    }

                    btn.Click += Btn_Click;
                    keyPanel.Controls.Add(btn);

                    x += btnWidth + padding;
                }
                y += rowHeight + padding;
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                string key = btn.Tag.ToString();

                if (_isFirstInput && key != "Shift" && key != "ToggleSymbol")
                {
                    if (key == "<-" || key == "CLR")
                        _currentInput = "";
                    else
                        _currentInput = "";

                    _isFirstInput = false;
                }

                switch (key)
                {
                    case "<-":
                        if (_currentInput.Length > 0)
                            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                        break;

                    case "CLR":
                        _currentInput = "";
                        break;

                    case "OK":
                        _targetTextBox.Text = _currentInput;
                        _targetTextBox.SelectionStart = _targetTextBox.Text.Length;
                        _targetTextBox.SelectionLength = 0;
                        OkClicked?.Invoke();
                        this.Close();
                        return;

                    case "Shift":
                        _isShifted = !_isShifted;
                        btnShift.BackColor = _isShifted ? btnShiftColor : btnDefaultColor;
                        btnShift.Text = _isShifted ? "SHIFT" : "Shift";
                        break;

                    case "ToggleSymbol":
                        _isSymbolMode = !_isSymbolMode;
                        if (!_isSymbolMode) _isShifted = false;
                        GenerateKeys();
                        break;

                    case "Space":
                        _currentInput += " ";
                        break;

                    default:
                        if (key.Length == 1 && char.IsLetter(key[0]))
                            _currentInput += _isShifted ? key.ToUpper() : key.ToLower();
                        else
                            _currentInput += key;
                        break;
                }

                txtDisplay.Text = _currentInput;

                if (_isFirstInput)
                {
                    txtDisplay.SelectAll();
                }
                else
                {
                    txtDisplay.SelectionLength = 0;
                    txtDisplay.SelectionStart = txtDisplay.Text.Length;
                }
            }
        }

        public void ShowBelow(Control control)
        {
            Point screenPoint = control.PointToScreen(new Point(0, control.Height));

            Screen currentScreen = Screen.FromControl(control);
            if (screenPoint.Y + this.Height > currentScreen.WorkingArea.Height)
                screenPoint.Y = control.PointToScreen(Point.Empty).Y - this.Height;

            if (screenPoint.X + this.Width > currentScreen.WorkingArea.Width)
                screenPoint.X = currentScreen.WorkingArea.Right - this.Width;

            this.Location = screenPoint;
            this.Show();

            txtDisplay.SelectAll();
        }

        // ====================================================================
        // 【新增】自定义不可点击的 TextBox 类
        // ====================================================================
        private class NonClickableTextBox : TextBox
        {
            protected override void WndProc(ref Message m)
            {
                // 鼠标左键按下
                const int WM_LBUTTONDOWN = 0x0201;
                // 鼠标左键双击
                const int WM_LBUTTONDBLCLK = 0x0203;
                // 鼠标右键按下 (连右键菜单也一起屏蔽)
                const int WM_RBUTTONDOWN = 0x0204;

                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_RBUTTONDOWN)
                {
                    // 直接返回，不调用基类的 WndProc，这样控件就不会响应点击获取焦点
                    return;
                }

                base.WndProc(ref m);
            }
        }
    }
}
