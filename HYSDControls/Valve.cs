using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ValveDemo
{
    /// <summary>
    /// 阀门控件：两个左右对称的三角形组成（蝶阀符号）
    /// </summary>
    [DefaultProperty("Color")]
    public partial class Valve : Control
    {
        private Color _color = Color.Silver;

        public Valve()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            Size = new Size(60, 40);
        }

        [Category("外观")]
        [Description("阀门颜色")]
        public Color Color
        {
            get => _color;
            set { _color = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = Width / 2f;
            float cy = Height / 2f;

            // 左三角 ▷（尖朝右）
            using (var brush = new SolidBrush(_color))
            using (var path = new GraphicsPath())
            {
                path.AddLine(0, 0, cx, cy);
                path.AddLine(cx, cy, 0, Height);
                path.CloseFigure();
                g.FillPath(brush, path);
            }

            // 右三角 ◁（尖朝左）
            using (var brush = new SolidBrush(_color))
            using (var path = new GraphicsPath())
            {
                path.AddLine(Width, 0, cx, cy);
                path.AddLine(cx, cy, Width, Height);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
