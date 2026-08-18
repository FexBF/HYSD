using System;
using System.Drawing;
using System.Windows.Forms;

namespace HYSDControls
{
    public class SquareLedControl : Control
    {
        private bool _isOn;
        private Color _onColor = Color.Lime;
        private Color _offColor = Color.DarkRed;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)          // ★ 值变化才重绘
                {
                    _isOn = value;
                    Invalidate();
                }
            }
        }

        public Color OnColor
        {
            get => _onColor;
            set { if (_onColor != value) { _onColor = value; Invalidate(); } }
        }

        public Color OffColor
        {
            get => _offColor;
            set { if (_offColor != value) { _offColor = value; Invalidate(); } }
        }

        public SquareLedControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            Size = new Size(30, 30);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Size = new Size(Width, Width);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // ★ 不调 base.OnPaint，配合 UserPaint 避免系统画背景
            Color currentColor = _isOn ? _onColor : _offColor;
            using (var brush = new SolidBrush(currentColor))   // ★ Dispose 防泄漏
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            IsOn = !IsOn;
        }
    }

}
