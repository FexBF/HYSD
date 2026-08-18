using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomLineControls
{
    public enum LineDirection
    {
        Horizontal,
        Vertical
    }

    public enum ArrowStyleType
    {
        FilledTriangle,  // 实心三角
        HollowTriangle,  // 空心三角
        Open,            // 开放式 (V型)
        Diamond,         // 菱形
        Circle           // 圆点
    }


    //[ToolboxItem(false)]
    public abstract class LineControlBase : Control
    {
        private Color _lineColor = Color.FromArgb(50, 50, 50);
        private int _lineWidth = 2;
        private DashStyle _dashStyle = DashStyle.Solid;

        [Category("线条外观")]
        [Description("线条颜色")]
        public Color LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; Invalidate(); }
        }

        [Category("线条外观")]
        [Description("线条宽度（像素）")]
        public int LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = Math.Max(1, value); Invalidate(); }
        }

        [Category("线条外观")]
        [Description("线条样式")]
        public DashStyle DashStyle
        {
            get { return _dashStyle; }
            set { _dashStyle = value; Invalidate(); }
        }

        protected LineControlBase()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            DrawLine(e.Graphics);
        }

        protected abstract void DrawLine(Graphics g);

        protected Pen CreatePen()
        {
            Pen pen = new Pen(_lineColor, _lineWidth);
            pen.DashStyle = _dashStyle;
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }

        protected void DrawArrowHead(Graphics g, PointF tip, PointF direction, float size)
        {
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            float halfAngle = (float)Math.PI / 6f;

            PointF left = new PointF(
                tip.X - size * (float)Math.Cos(angle - halfAngle),
                tip.Y - size * (float)Math.Sin(angle - halfAngle));
            PointF right = new PointF(
                tip.X - size * (float)Math.Cos(angle + halfAngle),
                tip.Y - size * (float)Math.Sin(angle + halfAngle));

            using (SolidBrush brush = new SolidBrush(_lineColor))
            {
                g.FillPolygon(brush, new PointF[] { tip, left, right });
            }
        }
    }

    [ToolboxBitmap(typeof(Control))]
    [Description("直线控件（水平或垂直）")]
    public class StraightLineControl : LineControlBase
    {
        private LineDirection _direction = LineDirection.Horizontal;

        [Category("线条行为")]
        [Description("直线方向：水平或垂直")]
        [DefaultValue(LineDirection.Horizontal)]
        public LineDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                Size = new Size(Size.Height, Size.Width);
                Invalidate();
            }
        }

        protected override void DrawLine(Graphics g)
        {
            using (Pen pen = CreatePen())
            {
                if (_direction == LineDirection.Horizontal)
                {
                    float y = Height / 2f;
                    g.DrawLine(pen, 0, y, Width, y);
                }
                else
                {
                    float x = Width / 2f;
                    g.DrawLine(pen, x, 0, x, Height);
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int thickness = LineWidth + 6;
            if (_direction == LineDirection.Horizontal)
                return new Size(200, thickness);
            else
                return new Size(thickness, 200);
        }
    }

    [ToolboxBitmap(typeof(Control))]
    [Description("斜线控件")]
    public class DiagonalLineControl : LineControlBase
    {
        private float _startX = 0f;
        private float _startY = 1f;
        private float _endX = 1f;
        private float _endY = 0f;

        [Category("线条位置")]
        [DefaultValue(0f)]
        public float StartX { get { return _startX; } set { _startX = Clamp01(value); Invalidate(); } }

        [Category("线条位置")]
        [DefaultValue(1f)]
        public float StartY { get { return _startY; } set { _startY = Clamp01(value); Invalidate(); } }

        [Category("线条位置")]
        [DefaultValue(1f)]
        public float EndX { get { return _endX; } set { _endX = Clamp01(value); Invalidate(); } }

        [Category("线条位置")]
        [DefaultValue(0f)]
        public float EndY { get { return _endY; } set { _endY = Clamp01(value); Invalidate(); } }

        private float Clamp01(float v) { return Math.Max(0f, Math.Min(1f, v)); }

        public void SetTopLeftToBottomRight() { StartX = 0; StartY = 0; EndX = 1; EndY = 1; }
        public void SetBottomLeftToTopRight() { StartX = 0; StartY = 1; EndX = 1; EndY = 0; }

        protected override void DrawLine(Graphics g)
        {
            using (Pen pen = CreatePen())
            {
                PointF start = new PointF(_startX * Width, _startY * Height);
                PointF end = new PointF(_endX * Width, _endY * Height);
                g.DrawLine(pen, start, end);
            }
        }

        public override Size GetPreferredSize(Size proposedSize) { return new Size(200, 150); }
    }

    [ToolboxBitmap(typeof(Control))]
    [Description("双箭头控件")]
    public class DoubleArrowControl : LineControlBase
    {
        private float _startX = 0f;
        private float _startY = 0.5f;
        private float _endX = 1f;
        private float _endY = 0.5f;
        private float _arrowSize = 14f;
        private LineDirection _direction = LineDirection.Horizontal;
        private ArrowStyleType _arrowStyle = ArrowStyleType.FilledTriangle;

        [Category("箭头行为")]
        [Description("箭头方向：水平或垂直")]
        [DefaultValue(LineDirection.Horizontal)]
        public LineDirection Direction
        {
            get { return _direction; }
            set
            {
                if (_direction == value) return;
                _direction = value;
                Size = new Size(Size.Height, Size.Width);
                if (value == LineDirection.Horizontal)
                {
                    _startX = 0f; _startY = 0.5f;
                    _endX = 1f; _endY = 0.5f;
                }
                else
                {
                    _startX = 0.5f; _startY = 0f;
                    _endX = 0.5f; _endY = 1f;
                }
                Invalidate();
            }
        }

        [Category("箭头外观")]
        [Description("箭头样式")]
        [DefaultValue(ArrowStyleType.FilledTriangle)]
        public ArrowStyleType ArrowStyle
        {
            get { return _arrowStyle; }
            set { _arrowStyle = value; Invalidate(); }
        }

        [Category("箭头位置")]
        [DefaultValue(0f)]
        public float StartX { get { return _startX; } set { _startX = Clamp01(value); Invalidate(); } }

        [Category("箭头位置")]
        [DefaultValue(0.5f)]
        public float StartY { get { return _startY; } set { _startY = Clamp01(value); Invalidate(); } }

        [Category("箭头位置")]
        [DefaultValue(1f)]
        public float EndX { get { return _endX; } set { _endX = Clamp01(value); Invalidate(); } }

        [Category("箭头位置")]
        [DefaultValue(0.5f)]
        public float EndY { get { return _endY; } set { _endY = Clamp01(value); Invalidate(); } }

        [Category("箭头外观")]
        [DefaultValue(14f)]
        public float ArrowSize
        {
            get { return _arrowSize; }
            set { _arrowSize = Math.Max(4f, value); Invalidate(); }
        }

        private float Clamp01(float v) { return Math.Max(0f, Math.Min(1f, v)); }

        protected override void DrawLine(Graphics g)
        {
            PointF start = new PointF(_startX * Width, _startY * Height);
            PointF end = new PointF(_endX * Width, _endY * Height);

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            if (length < 1f) return;

            float ux = dx / length;
            float uy = dy / length;

            // 开放式和圆点样式的线段直接画到底，不需要缩短
            bool needShorten = (_arrowStyle != ArrowStyleType.Open && _arrowStyle != ArrowStyleType.Circle);

            PointF lineStart = start;
            PointF lineEnd = end;

            if (needShorten)
            {
                // 为封闭式箭头腾出空间
                lineStart = new PointF(start.X + ux * _arrowSize, start.Y + uy * _arrowSize);
                lineEnd = new PointF(end.X - ux * _arrowSize, end.Y - uy * _arrowSize);
            }

            using (Pen pen = CreatePen())
            {
                g.DrawLine(pen, lineStart, lineEnd);
            }

            // 绘制两端箭头
            DrawCustomArrow(g, start, new PointF(-ux, -uy), _arrowSize);
            DrawCustomArrow(g, end, new PointF(ux, uy), _arrowSize);
        }


        private void DrawCustomArrow(Graphics g, PointF tip, PointF direction, float size)
        {
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            float halfAngle = (float)Math.PI / 6f; // 30°

            PointF left = new PointF(
                tip.X - size * (float)Math.Cos(angle - halfAngle),
                tip.Y - size * (float)Math.Sin(angle - halfAngle));
            PointF right = new PointF(
                tip.X - size * (float)Math.Cos(angle + halfAngle),
                tip.Y - size * (float)Math.Sin(angle + halfAngle));
            PointF back = new PointF(
                tip.X - size * (float)Math.Cos(angle),
                tip.Y - size * (float)Math.Sin(angle));

            if (_arrowStyle == ArrowStyleType.FilledTriangle)
            {
                using (SolidBrush brush = new SolidBrush(LineColor))
                {
                    g.FillPolygon(brush, new PointF[] { tip, left, right });
                }
            }
            else if (_arrowStyle == ArrowStyleType.HollowTriangle)
            {
                using (Pen pen = new Pen(LineColor, LineWidth))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawPolygon(pen, new PointF[] { tip, left, right });
                }
            }
            else if (_arrowStyle == ArrowStyleType.Open)
            {
                using (Pen pen = new Pen(LineColor, LineWidth))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, left, tip);
                    g.DrawLine(pen, tip, right);
                }
            }
            else if (_arrowStyle == ArrowStyleType.Diamond)
            {
                using (SolidBrush brush = new SolidBrush(LineColor))
                {
                    g.FillPolygon(brush, new PointF[] { tip, left, back, right });
                }
            }
            else if (_arrowStyle == ArrowStyleType.Circle)
            {
                float radius = size / 2f;
                using (SolidBrush brush = new SolidBrush(LineColor))
                {
                    g.FillEllipse(brush, tip.X - radius, tip.Y - radius, radius * 2, radius * 2);
                }
            }
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (_direction == LineDirection.Horizontal)
                return new Size(250, 50);
            else
                return new Size(50, 250);
        }
    }


}
