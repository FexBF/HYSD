using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HYSDControls
{
    public class ToggleSwitch : Control
    {
        private bool _isChecked;
        private Color _onColor = Color.FromArgb(0, 120, 215);
        private Color _offColor = Color.FromArgb(180, 180, 180);

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Color OnColor { get => _onColor; set { _onColor = value; Invalidate(); } }
        public Color OffColor { get => _offColor; set { _offColor = value; Invalidate(); } }

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            Size = new Size(50, 30);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color currentColor = _isChecked ? _onColor : _offColor;

            // 统一内边距
            float padding = 5f;

            // 圆的尺寸：上下左右都留边距
            float circleDiameter = Height - padding * 2;
            float circleY = padding;

            // 槽的左右端点刚好在两个圆心位置，保证圆能盖住槽口
            float trackX = padding + circleDiameter / 2f;
            float trackWidth = Width - padding * 2 - circleDiameter;
            float trackHeight = circleDiameter * 0.6f;
            float trackY = padding + (circleDiameter - trackHeight) / 2f;

            // 1. 画长条槽
            using (GraphicsPath trackPath = CreateRoundRect(trackX, trackY, trackWidth, trackHeight, trackHeight / 2f))
            using (SolidBrush trackBrush = new SolidBrush(currentColor))
            {
                g.FillPath(trackBrush, trackPath);
            }

            // 2. 画圆形滑块
            float circleX = _isChecked ? (Width - padding - circleDiameter) : padding;
            using (SolidBrush circleBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(circleBrush, circleX, circleY, circleDiameter, circleDiameter);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            IsChecked = !IsChecked;
        }

        private GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(x, y, radius, radius, 180, 90);
            path.AddArc(x + width - radius, y, radius, radius, 270, 90);
            path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
            path.AddArc(x, y + height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
