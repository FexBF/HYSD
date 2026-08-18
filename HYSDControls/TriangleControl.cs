using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TriangleDemo
{
    [DefaultProperty("Color")]
    public class TriangleControl : Control
    {
        private Color _color = Color.FromArgb(0, 120, 212);
        private float _rotation; // 度：0=上，90=右，180=下，270=左

        public TriangleControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            Size = new Size(40, 40);
        }

        [Category("外观")]
        public Color Color
        {
            get => _color;
            set { _color = value; Invalidate(); }
        }

        [Category("行为")]
        [Description("旋转角度：0=上 90=右 180=下 270=左")]
        [DefaultValue(0f)]
        public float Rotation
        {
            get => _rotation;
            set { _rotation = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = Width / 2f;
            float cy = Height / 2f;
            float r = Math.Min(cx, cy) - 2f;

            // 以(0,0)为圆心定义朝上的三角，再平移旋转
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_rotation);

            using (var brush = new SolidBrush(_color))
            using (var path = new GraphicsPath())
            {
                path.AddLine(0, -r, r * 0.866f, r * 0.5f);
                path.AddLine(r * 0.866f, r * 0.5f, -r * 0.866f, r * 0.5f);
                path.CloseFigure();
                g.FillPath(brush, path);
            }

            g.ResetTransform();
        }
    }
}
