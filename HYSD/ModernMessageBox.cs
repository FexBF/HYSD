using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HYSD
{
    public class ModernMessageBox : Form
    {
        /// <summary>
        /// 显示现代消息框
        /// </summary>
        public static DialogResult Show(string message, string title = "提示",
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
            MessageBoxIcon icon = MessageBoxIcon.None)
        {
            try
            {
                using (var box = new ModernMessageBox(title, message, buttons, icon))
                {
                    box.StartPosition = FormStartPosition.CenterScreen;
                    box.ShowDialog();
                    return box.DialogResult;
                }
            }
            catch (Exception ex)
            {
                // 兜底保护
                MessageBox.Show($"弹窗渲染异常:\n{ex.Message}\n\n原始消息:\n{message}", title, buttons, icon);
                return DialogResult.None;
            }
        }

        private ModernMessageBox(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.ForeColor = Color.FromArgb(50, 50, 50);
            this.Font = new Font("微软雅黑", 9F);
            this.MinimumSize = new Size(380, 200);
            this.TopMost = true;
            this.DoubleBuffered = true; // 双缓冲防闪烁
            this.Padding = new Padding(0); // 强制无内边距

            InitializeComponents(title, message, buttons, icon);
        }

        // 画 1px 边框遮盖系统白边
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            }
        }

        private void InitializeComponents(string title, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            // 主容器，0边距
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));  // 标题栏
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // 内容区
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // 按钮区

            // === 1. 标题栏 ===
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb((2635155 & 0xFF0000) >> 16, (2635155 & 0xFF00) >> 8, 2635155 & 0xFF),
                Margin = new Padding(0) // 确保无间距
            };
            headerPanel.MouseDown += HeaderPanel_MouseDown;

            Label lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0)
            };
            lblTitle.MouseDown += HeaderPanel_MouseDown;

            Button btnClose = new Button
            {
                Text = "x",
                Dock = DockStyle.Right,
                Width = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial Rounded MT Bold", 14F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            headerPanel.Controls.Add(btnClose);
            headerPanel.Controls.Add(lblTitle);

            // === 2. 内容区 ===
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(0)
            };

            if (icon != MessageBoxIcon.None)
            {
                PictureBox picIcon = new PictureBox
                {
                    Size = new Size(32, 32),
                    Location = new Point(20, 20),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = DrawModernIcon(icon)
                };
                contentPanel.Controls.Add(picIcon);
            }

            Label lblMessage = new Label
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                Font = new Font("微软雅黑", 10F),
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = Color.Transparent
            };

            if (icon != MessageBoxIcon.None)
                lblMessage.Location = new Point(65, 20);
            else
                lblMessage.Location = new Point(20, 20);

            contentPanel.Controls.Add(lblMessage);

            // === 3. 按钮区 ===
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245),
                Margin = new Padding(0)
            };

            // 按钮尺寸与间距常量
            int btnWidth = 90;
            int btnHeight = 35;
            int gap = 12;  // 两个按钮之间的间距

            if (buttons == MessageBoxButtons.OK)
            {
                AddButton(buttonPanel, "确定", DialogResult.OK, true, btnWidth, btnHeight);
            }
            else if (buttons == MessageBoxButtons.OKCancel)
            {
                AddButton(buttonPanel, "确定", DialogResult.OK, true, btnWidth, btnHeight);
                AddButton(buttonPanel, "取消", DialogResult.Cancel, false, btnWidth, btnHeight);
            }
            else if (buttons == MessageBoxButtons.YesNo)
            {
                AddButton(buttonPanel, "是", DialogResult.Yes, true, btnWidth, btnHeight);
                AddButton(buttonPanel, "否", DialogResult.No, false, btnWidth, btnHeight);
            }

            // 存储间距供居中计算使用，并绑定 Resize 事件自动居中
            buttonPanel.Tag = gap;
            buttonPanel.Resize += (s, e) => CenterButtons(buttonPanel, gap);

            // 组装
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(contentPanel, 0, 1);
            mainLayout.Controls.Add(buttonPanel, 0, 2);
            this.Controls.Add(mainLayout);

            // 动态计算高度
            int textHeight = lblMessage.PreferredHeight;
            int neededHeight = 40 + 60 + Math.Max(60, textHeight + 50);
            this.Height = neededHeight;
        }

        /// <summary>
        /// 将按钮区内的所有按钮水平居中、垂直居中排列
        /// </summary>
        private void CenterButtons(Panel buttonPanel, int gap)
        {
            int count = buttonPanel.Controls.Count;
            if (count == 0) return;

            int btnWidth = buttonPanel.Controls[0].Width;
            int btnHeight = buttonPanel.Controls[0].Height;

            // 所有按钮的总宽度 = 按钮数 × 按钮宽 + 间距数 × 间距
            int totalWidth = count * btnWidth + (count - 1) * gap;
            // 起始 X = (面板宽 - 总宽度) / 2
            int startX = (buttonPanel.ClientSize.Width - totalWidth) / 2;
            // 垂直居中 Y = (面板高 - 按钮高) / 2
            int yPos = (buttonPanel.ClientSize.Height - btnHeight) / 2;

            for (int i = 0; i < count; i++)
            {
                buttonPanel.Controls[i].Location = new Point(startX + i * (btnWidth + gap), yPos);
            }
        }

        private void AddButton(Panel parent, string text, DialogResult result, bool isPrimary, int btnWidth, int btnHeight)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(0, 0), // 初始位置，后续由 CenterButtons 计算
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("微软雅黑", 9F),
                Tag = result
            };

            if (isPrimary)
            {
                btn.BackColor = Color.FromArgb(0, 122, 204);
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.FromArgb(0, 122, 204);
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(50, 50, 50);
                btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            }

            btn.Click += (s, e) =>
            {
                this.DialogResult = (DialogResult)btn.Tag;
                this.Close();
            };

            parent.Controls.Add(btn);
        }

        // 自绘扁平图标
        private Bitmap DrawModernIcon(MessageBoxIcon icon)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (icon == MessageBoxIcon.Information)
                {
                    g.FillEllipse(Brushes.DodgerBlue, 2, 2, 28, 28);
                    Font f = new Font("微软雅黑", 18F, FontStyle.Bold);
                    g.DrawString("i", f, Brushes.White, 6, -2);
                }
                else if (icon == MessageBoxIcon.Warning)
                {
                    Point[] points = { new Point(16, 2), new Point(30, 28), new Point(2, 28) };
                    g.FillPolygon(Brushes.Orange, points);
                    Font f = new Font("微软雅黑", 16F, FontStyle.Bold);
                    g.DrawString("!", f, Brushes.White, 11, 4);
                }
                else if (icon == MessageBoxIcon.Error)
                {
                    g.FillEllipse(Brushes.Red, 2, 2, 28, 28);
                    Font f = new Font("微软雅黑", 18F, FontStyle.Bold);
                    g.DrawString("X", f, Brushes.White, 5, 2);
                }
                else if (icon == MessageBoxIcon.Question)
                {
                    g.FillEllipse(Brushes.DodgerBlue, 2, 2, 28, 28);
                    Font f = new Font("微软雅黑", 18F, FontStyle.Bold);
                    g.DrawString("?", f, Brushes.White, 6, 0);
                }
            }
            return bmp;
        }

        // 拖动标题栏
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int HTCAPTION = 0x02;

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}
