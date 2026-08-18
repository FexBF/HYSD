using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LockDemo
{
    [DefaultProperty("LockedColor")]
    public class LockControl : Control
    {
        private Color _lockedColor = Color.FromArgb(0, 120, 212);
        private Color _unlockedColor = Color.FromArgb(180, 180, 180);
        private bool _isLocked = true;

        // 动画
        private readonly Timer _timer = new Timer { Interval = 16 };
        private float _currentProgress; // 0=上锁，1=解锁
        private float _targetProgress;
        private float _speed = 0.08f;

        public LockControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            Size = new Size(40, 50);

            _timer.Tick += (s, e) =>
            {
                if (Math.Abs(_currentProgress - _targetProgress) < 0.01f)
                {
                    _currentProgress = _targetProgress;
                    _timer.Stop();
                }
                else
                {
                    // 缓动插值
                    _currentProgress += (_targetProgress - _currentProgress) * _speed;
                }
                Invalidate();
            };
        }

        [Category("外观")]
        [Description("上锁时的颜色")]
        public Color LockedColor
        {
            get => _lockedColor;
            set { _lockedColor = value; Invalidate(); }
        }

        [Category("外观")]
        [Description("解锁时的颜色")]
        public Color UnlockedColor
        {
            get => _unlockedColor;
            set { _unlockedColor = value; Invalidate(); }
        }

        [Category("行为")]
        [Description("当前是否上锁")]
        [DefaultValue(true)]
        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                if (_isLocked == value) return;
                _isLocked = value;
                _targetProgress = value ? 0f : 1f;
                if (!_timer.Enabled) _timer.Start();
            }
        }

        [Category("行为")]
        [Description("动画速度")]
        [DefaultValue(0.08f)]
        public float AnimationSpeed
        {
            get => _speed;
            set { _speed = Math.Max(0.01f, value); }
        }

        /// <summary>切换上锁/解锁状态</summary>
        public void Toggle() => IsLocked = !IsLocked;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float w = Width;
            float bodyW = w * 0.7f;
            float bodyH = w * 0.55f;
            float bodyX = (w - bodyW) / 2f;
            float bodyY = Height - bodyH - 2f;

            float shackleW = bodyW * 0.6f;
            float shackleH = w * 0.45f;
            float shackleX = (w - shackleW) / 2f;

            // 解锁时锁环向上偏移的距离
            float maxLift = w * 0.2f;
            float lift = maxLift * EaseOutQuad(_currentProgress);
            float shackleY = bodyY - shackleH + lift + 4f;

            // 根据进度插值颜色
            Color currentColor = InterpolateColor(_lockedColor, _unlockedColor, _currentProgress);

            // 1. 画锁环 (U型粗线)
            float penWidth = Math.Max(3f, w * 0.1f);
            using (var pen = new Pen(currentColor, penWidth))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, shackleX, shackleY, shackleW, shackleH, 180, 180);
            }

            // 2. 画锁体 (圆角矩形)
            float radius = Math.Max(2f, bodyW * 0.15f);
            using (var path = CreateRoundedRectPath(bodyX, bodyY, bodyW, bodyH, radius))
            using (var brush = new SolidBrush(currentColor))
            {
                g.FillPath(brush, path);
            }

            // 3. 画锁孔
            float holeR = Math.Max(1.5f, w * 0.06f);
            float holeY = bodyY + bodyH * 0.4f;
            using (var holeBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillEllipse(holeBrush, w / 2f - holeR, holeY - holeR, holeR * 2, holeR * 2);

                // 锁孔下方的横线
                float slotW = holeR * 1.2f;
                float slotH = holeR * 1.5f;
                g.FillRectangle(holeBrush, w / 2f - slotW / 2f, holeY + holeR, slotW, slotH);
            }
        }

        private GraphicsPath CreateRoundedRectPath(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private float EaseOutQuad(float t) => t * (2f - t);

        private Color InterpolateColor(Color c1, Color c2, float t)
        {
            return Color.FromArgb(
                (int)(c1.A + (c2.A - c1.A) * t),
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t)
            );
        }

        protected override void Dispose(bool disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
