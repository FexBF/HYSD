using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuarterRingDemo
{
    /// <summary>
    /// 四等分圆环，对角颜色同步，带流动效果
    /// </summary>
    [DefaultProperty("ColorA")]
    public class QuarterRingControl : Control
    {
        private Color _colorA = Color.FromArgb(0, 120, 212);
        private Color _colorB = Color.FromArgb(255, 165, 2);
        private float _ringWidth = 28f;
        private float _gapAngle = 4f;
        private float _startAngle = -90f;
        private bool _roundedCaps = true;
        private bool _reversed = false;

        // 流动
        private readonly Timer _timer = new Timer();
        private float _flowOffset;
        private bool _flowing;
        private float _flowSpeed = 1.5f;

        public QuarterRingControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true);
            Size = new Size(200, 200);
            BackColor = Color.Transparent;

            _timer.Interval = 16; // ~60fps
            _timer.Tick += (s, e) =>
            {
                float direction = _reversed ? -1 : 1;
                _flowOffset = (_flowOffset + _flowSpeed * direction) % 360f;
                if (_flowOffset < 0) _flowOffset += 360f;
                Invalidate();
            };
        }

        [Category("外观")]
        [Description("对角颜色：第1段 + 第3段")]
        public Color ColorA
        {
            get => _colorA;
            set { _colorA = value; Invalidate(); }
        }

        [Category("外观")]
        [Description("对角颜色：第2段 + 第4段")]
        public Color ColorB
        {
            get => _colorB;
            set { _colorB = value; Invalidate(); }
        }

        [Category("外观")]
        [Description("圆环线宽")]
        [DefaultValue(28f)]
        public float RingWidth
        {
            get => _ringWidth;
            set { _ringWidth = Math.Max(4f, value); Invalidate(); }
        }

        [Category("外观")]
        [Description("间隙角度，0=无间隙")]
        [DefaultValue(4f)]
        public float GapAngle
        {
            get => _gapAngle;
            set { _gapAngle = Math.Max(0f, Math.Min(45f, value)); Invalidate(); }
        }

        [Category("外观")]
        [Description("静态起始角度，-90=12点钟方向")]
        [DefaultValue(-90f)]
        public float StartAngle
        {
            get => _startAngle;
            set { _startAngle = value; Invalidate(); }
        }

        [Category("外观")]
        [Description("圆角端点")]
        [DefaultValue(true)]
        public bool RoundedCaps
        {
            get => _roundedCaps;
            set { _roundedCaps = value; Invalidate(); }
        }

        [Category("流动")]
        [Description("是否启用流动")]
        [DefaultValue(false)]
        public bool Flowing
        {
            get => _flowing;
            set
            {
                _flowing = value;
                if (value)
                    _timer.Start();
                else
                    _timer.Stop();
            }
        }

        [Category("流动")]
        [Description("流动速度（度/帧）")]
        [DefaultValue(1.5f)]
        public float FlowSpeed
        {
            get => _flowSpeed;
            set { _flowSpeed = Math.Max(0.1f, value); }
        }

        [Category("流动")]
        [Description("是否反转流动方向")]
        [DefaultValue(false)]
        public bool Reversed
        {
            get => _reversed;
            set { _reversed = value; }
        }

        /// <summary>
        /// 切换反转状态
        /// </summary>
        public void ToggleReverse()
        {
            _reversed = !_reversed;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            float cx = Width / 2f;
            float cy = Height / 2f;
            float maxR = Math.Min(cx, cy) - 4f;
            float radius = Math.Max(_ringWidth / 2f + 1f, maxR - _ringWidth / 2f);
            var rect = new RectangleF(cx - radius, cy - radius, radius * 2f, radius * 2f);

            float totalGap = _gapAngle * 4f;
            float segAngle = Math.Max(0.1f, (360f - totalGap) / 4f);
            var cap = _roundedCaps ? LineCap.Round : LineCap.Flat;

            var colors = new[] { _colorA, _colorB, _colorA, _colorB };

            // 加上流动偏移
            float angle = _startAngle + _flowOffset;

            for (int i = 0; i < 4; i++)
            {
                // 阴影
                using (var sp = new Pen(Color.FromArgb(30, 0, 0, 0), _ringWidth))
                {
                    sp.StartCap = sp.EndCap = cap;
                    float off = Math.Max(2f, _ringWidth * 0.06f);
                    g.DrawArc(sp, rect.X + off, rect.Y + off, rect.Width, rect.Height,
                        angle, segAngle);
                }

                // 圆环段
                using (var pen = new Pen(colors[i], _ringWidth))
                {
                    pen.StartCap = pen.EndCap = cap;
                    g.DrawArc(pen, rect, angle, segAngle);
                }

                angle += segAngle + _gapAngle;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            // 控件不可见（页面切走）时停动画，可见且 Flowing=true 时恢复
            if (!Visible)
                _timer.Stop();
            else if (_flowing)
                _timer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
