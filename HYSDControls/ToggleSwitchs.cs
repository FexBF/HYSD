using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text; // 【新增】引入文本渲染命名空间
using System.Windows.Forms;

namespace HYSDControls // 请改成你自己的命名空间
{
    // 开关样式枚举
    public enum ToggleStyle
    {
        /// <summary>
        /// 胶囊形（默认大圆角）
        /// </summary>
        Rounded,
        /// <summary>
        /// 长方形（微圆角/直角）
        /// </summary>
        Rectangle
    }

    public class ToggleSwitchs : Control
    {
        #region 属性和字段

        private bool isOn = false;
        private bool isPlcConnected = true;
        private float sliderPosition = 0f;
        private ToggleStyle style = ToggleStyle.Rounded;

        // 颜色配置
        private Color onColor = Color.FromArgb(0, 192, 0);
        private Color offColor = Color.FromArgb(100, 100, 100);
        private Color disabledColor = Color.FromArgb(100, 100, 100);
        private Color sliderColor = Color.White;

        // 动画相关
        private Timer animTimer;

        // 防抖锁定相关
        private Timer commitTimer;
        private bool isCommitting = false;

        /// <summary>
        /// 开关状态。封装防抖：点击后锁定期间忽略外部赋值。
        /// </summary>
        [Category("开关")]
        [Description("当前开关状态，True为ON，False为OFF")]
        public bool IsOn
        {
            get => isOn;
            set
            {
                // 如果正在防抖锁定期间，直接忽略PLC读取回来的旧值，防止闪烁
                if (isCommitting) return;

                if (isOn != value)
                {
                    isOn = value;
                    StartAnimation();
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// PLC连接状态。掉线时控件变灰且不可点击。
        /// </summary>
        [Category("开关")]
        [Description("PLC是否在线，掉线时禁止点击操作")]
        public bool IsPlcConnected
        {
            get => isPlcConnected;
            set
            {
                if (isPlcConnected != value)
                {
                    isPlcConnected = value;
                    this.Cursor = isPlcConnected ? Cursors.Hand : Cursors.No;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 控件的外观样式。
        /// </summary>
        [Category("外观")]
        [Description("设置开关的形状样式：Rounded为胶囊形，Rectangle为长方形")]
        public ToggleStyle Style
        {
            get => style;
            set
            {
                if (style != value)
                {
                    style = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 点击切换后的防抖锁定时间(毫秒)。
        /// </summary>
        [Category("开关")]
        [Description("防抖锁定时间(毫秒)，防止PLC读取旧值覆盖写入操作，0为不锁定")]
        public int CommitLockTime { get; set; } = 300;

        [Category("外观")]
        [Description("ON状态的背景颜色")]
        public Color OnColor { get => onColor; set { onColor = value; Invalidate(); } }

        [Category("外观")]
        [Description("OFF状态的背景颜色")]
        public Color OffColor { get => offColor; set { offColor = value; Invalidate(); } }

        [Category("外观")]
        [Description("掉线禁用状态的背景颜色")]
        public Color DisabledColor { get => disabledColor; set { disabledColor = value; Invalidate(); } }

        [Category("外观")]
        [Description("滑块颜色")]
        public Color SliderColor { get => sliderColor; set { sliderColor = value; Invalidate(); } }

        /// <summary>
        /// 当用户点击切换开关时触发，用于向PLC发送写入指令。
        /// </summary>
        [Category("动作")]
        [Description("当用户点击切换开关时触发")]
        public event EventHandler Toggled;

        #endregion

        public ToggleSwitchs()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(80, 30);
            this.Cursor = Cursors.Hand;
            this.MinimumSize = new Size(40, 20);

            animTimer = new Timer();
            animTimer.Interval = 15;
            animTimer.Tick += AnimTimer_Tick;

            commitTimer = new Timer();
            commitTimer.Tick += CommitTimer_Tick;
        }

        #region 交互与防抖逻辑

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (!isPlcConnected || isCommitting) return;

            this.isOn = !this.isOn;
            Toggled?.Invoke(this, EventArgs.Empty);
            StartAnimation();

            if (CommitLockTime > 0)
            {
                isCommitting = true;
                commitTimer.Interval = CommitLockTime;
                commitTimer.Start();
            }
        }

        private void CommitTimer_Tick(object sender, EventArgs e)
        {
            commitTimer.Stop();
            isCommitting = false;
        }

        #endregion

        #region 动画逻辑

        private void StartAnimation()
        {
            if (!animTimer.Enabled)
            {
                animTimer.Start();
            }
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            float padding = Math.Max(2f, this.Height * 0.08f);
            float sliderW = this.Height - padding * 2;
            float targetX = isOn ? this.Width - sliderW - padding : padding;

            sliderPosition += (targetX - sliderPosition) * 0.3f;

            if (Math.Abs(sliderPosition - targetX) < 0.5f)
            {
                sliderPosition = targetX;
                animTimer.Stop();
            }

            Invalidate();
        }

        #endregion

        #region 绘制逻辑

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 【关键修复1】设置文本渲染模式为ClearType，使字体清晰锐利，不再发虚模糊
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            float w = this.Width;
            float h = this.Height;

            float padding = Math.Max(2f, h * 0.08f);
            float sliderW = h - padding * 2;
            float sliderH = sliderW;

            float bgRadius = 0f;
            float sliderRadius = 0f;

            if (style == ToggleStyle.Rounded)
            {
                bgRadius = h / 2f;
                sliderRadius = sliderW / 2f;
            }
            else
            {
                bgRadius = padding * 1.5f;
                sliderRadius = padding;
            }

            Color bgColor = isPlcConnected ? (isOn ? onColor : offColor) : disabledColor;
            using (GraphicsPath bgPath = GetRoundedRectPath(0, 0, w, h, bgRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, bgPath);
                }
            }

            string text = isOn ? "ON" : "OFF";

            float fontSize = h * 0.4f;
            if (fontSize < 6f) fontSize = 6f;

            using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(text, font);

                // 计算Y坐标并四舍五入
                float textY = (h - textSize.Height) / 2f;
                int drawY = (int)Math.Round(textY);

                float textX;
                if (isOn)
                {
                    float leftAreaWidth = sliderPosition;
                    textX = (leftAreaWidth - textSize.Width) / 2f;
                }
                else
                {
                    float rightAreaStart = sliderPosition + sliderW;
                    float rightAreaWidth = w - rightAreaStart;
                    textX = rightAreaStart + (rightAreaWidth - textSize.Width) / 2f;
                }

                // 【关键修复2】计算X坐标并四舍五入，避免小数坐标导致字体模糊
                int drawX = (int)Math.Round(textX);

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    // 使用整数坐标绘制文字
                    g.DrawString(text, font, textBrush, drawX, drawY);
                }
            }

            using (GraphicsPath sliderPath = GetRoundedRectPath(sliderPosition, padding, sliderW, sliderH, sliderRadius))
            {
                using (SolidBrush sliderBrush = new SolidBrush(sliderColor))
                {
                    g.FillPath(sliderBrush, sliderPath);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(float x, float y, float w, float h, float r)
        {
            GraphicsPath path = new GraphicsPath();

            if (r > h / 2f) r = h / 2f;
            if (r > w / 2f) r = w / 2f;

            if (r <= 0f)
            {
                path.AddRectangle(new RectangleF(x, y, w, h));
            }
            else
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
            }
            return path;
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            float padding = Math.Max(2f, this.Height * 0.08f);
            float sliderW = this.Height - padding * 2;
            sliderPosition = isOn ? this.Width - sliderW - padding : padding;
            Invalidate();
        }
    }
}
