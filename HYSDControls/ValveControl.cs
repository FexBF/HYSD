using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ValveDemo
{
    /// <summary>
    /// 蝶阀控件：两个箭头朝内，开启时旋转180度
    /// </summary>
    [DefaultProperty("ClosedColor")]
    public class ValveControl : Control
    {
        private Color _openColor = Color.FromArgb(76, 175, 80);
        private Color _closedColor = Color.FromArgb(244, 67, 54);
        private bool _isOpen;

        private readonly Timer _timer = new Timer { Interval = 16 };
        private float _currentAngle;
        private float _targetAngle;
        private float _speed = 0.06f;

        public ValveControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            Size = new Size(40, 40);

            _timer.Tick += (s, e) =>
            {
                if (Math.Abs(_currentAngle - _targetAngle) < 0.5f)
                {
                    _currentAngle = _targetAngle;
                    _timer.Stop();
                }
                else
                {
                    _currentAngle += (_targetAngle - _currentAngle) * _speed;
                }
                Invalidate();
            };
        }

        [Category("外观")]
        public Color OpenColor
        {
            get => _openColor;
            set { _openColor = value; Invalidate(); }
        }

        [Category("外观")]
        public Color ClosedColor
        {
            get => _closedColor;
            set { _closedColor = value; Invalidate(); }
        }

        [Category("行为")]
        [DefaultValue(false)]
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value) return;
                _isOpen = value;
                _targetAngle = value ? 180f : 0f;
                if (!_timer.Enabled) _timer.Start();
            }
        }

        [Category("行为")]
        [DefaultValue(0.06f)]
        public float AnimationSpeed
        {
            get => _speed;
            set { _speed = Math.Max(0.01f, value); }
        }

        public void Toggle() => IsOpen = !IsOpen;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // 🌟 开启高质量绘图，消除锯齿和马赛克
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;      // 让曲线和边缘平滑
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // 让图片缩放平滑
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; // 让文字清晰无锯齿

            float cx = Width / 2f;
            float cy = Height / 2f;
            float halfW = cx - 3f;
            float halfH = cy - 3f;

            float progress = _currentAngle / 180f;
            Color color = InterpolateColor(_closedColor, _openColor, progress);

            g.TranslateTransform(cx, cy);
            g.RotateTransform(_currentAngle);

            // 左三角 ▷ 尖朝右
            using (var brush = new SolidBrush(color))
                g.FillPolygon(brush, new PointF[]
                {
                    new PointF(-halfW, -halfH),
                    new PointF(0, 0),
                    new PointF(-halfW, halfH)
                });

            // 右三角 ◁ 尖朝左
            using (var brush = new SolidBrush(color))
                g.FillPolygon(brush, new PointF[]
                {
                    new PointF(halfW, -halfH),
                    new PointF(0, 0),
                    new PointF(halfW, halfH)
                });

            g.ResetTransform();
        }

        private Color InterpolateColor(Color c1, Color c2, float t)
        {
            return Color.FromArgb(
                (int)(c1.A + (c2.A - c1.A) * t),
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t));
        }

        protected override void Dispose(bool disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
