using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace HYSDControls {
    public class AlarmIndicatorLight : Control
    {
        private Timer _blinkTimer;
        private bool _isFlashOn = true;

        public Color AlarmColor { get; set; } = Color.FromArgb(237, 173, 58);
        public Color SignColor { get; set; } = Color.FromArgb(40, 44, 52);
        public Color IdleColor { get; set; } = Color.FromArgb(60, 63, 70);

        private bool _isAlarming = false;

        // ★ 预渲染位图：亮态、暗态、空闲态
        private Bitmap _bmpOn;
        private Bitmap _bmpOff;
        private Bitmap _bmpIdle;

        public bool IsAlarming
        {
            get { return _isAlarming; }
            set
            {
                if (_isAlarming != value)
                {
                    _isAlarming = value;
                    if (_isAlarming)
                    {
                        _isFlashOn = true;
                        _blinkTimer.Start();
                        this.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        _blinkTimer.Stop();
                        _isFlashOn = true;
                        this.Cursor = Cursors.Default;
                    }
                    this.Invalidate();
                }
            }
        }

        public event EventHandler AlarmClicked;

        public AlarmIndicatorLight()
        {
            this.Size = new Size(40, 40);
            this.Cursor = Cursors.Default;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            _blinkTimer = new Timer();
            _blinkTimer.Interval = 1000;
            _blinkTimer.Tick += BlinkTimer_Tick;
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            _isFlashOn = !_isFlashOn;
            this.Invalidate();   // 只是触发 OnPaint，里面只做 DrawImage
        }

        // ★ 预渲染三张位图，尺寸变化时重建
        private void RebuildBitmaps()
        {
            _bmpOn?.Dispose();
            _bmpOff?.Dispose();
            _bmpIdle?.Dispose();

            _bmpOn = RenderBitmap(true, true);
            _bmpOff = RenderBitmap(true, false);
            _bmpIdle = RenderBitmap(false, false);
        }

        private Bitmap RenderBitmap(bool alarming, bool flashOn)
        {
            var bmp = new Bitmap(this.Width, this.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // 填背景
                using (var bgBrush = new SolidBrush(this.BackColor))
                    g.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

                float cx = this.Width / 2f;
                float cy = this.Height / 2f;
                float drawW = this.Width - 8f;
                float drawH = this.Height - 8f;

                Color currentTriColor = IdleColor;
                Color currentSignColor = Color.FromArgb(45, 48, 55);

                if (alarming)
                {
                    if (flashOn)
                    {
                        currentTriColor = AlarmColor;
                        currentSignColor = SignColor;

                        // 外发光
                        using (var glowPath = CreateTrianglePath(cx, cy, drawW, drawH, 5f))
                        using (var glowBrush = new SolidBrush(Color.FromArgb(70, AlarmColor)))
                            g.FillPath(glowBrush, glowPath);
                    }
                    else
                    {
                        currentTriColor = Color.FromArgb(50, AlarmColor);
                        currentSignColor = Color.FromArgb(30, SignColor);
                    }
                }

                // 三角形本体
                using (var triPath = CreateTrianglePath(cx, cy, drawW, drawH, 0f))
                using (var triBrush = new SolidBrush(currentTriColor))
                    g.FillPath(triBrush, triPath);

                // 感叹号
                using (var signBrush = new SolidBrush(currentSignColor))
                {
                    float lineW = Math.Max(2f, drawW * 0.1f);
                    float lineH = Math.Max(4f, drawH * 0.28f);
                    float lineTop = cy - drawH * 0.12f;
                    g.FillRectangle(signBrush, cx - lineW / 2f, lineTop, lineW, lineH);

                    float dotR = Math.Max(1.5f, drawW * 0.07f);
                    float dotY = cy + drawH * 0.26f;
                    g.FillEllipse(signBrush, cx - dotR, dotY - dotR, dotR * 2f, dotR * 2f);
                }
            }
            return bmp;
        }

        private GraphicsPath CreateTrianglePath(float cx, float cy, float drawW, float drawH, float expand)
        {
            var path = new GraphicsPath();
            var p1 = new PointF(cx, cy - drawH * 0.45f);
            var p2 = new PointF(cx - drawW * 0.52f, cy + drawH * 0.45f);
            var p3 = new PointF(cx + drawW * 0.52f, cy + drawH * 0.45f);
            path.AddLine(p1, p2);
            path.AddLine(p2, p3);
            path.CloseFigure();

            if (expand > 0)
            {
                using (var pen = new Pen(Color.Black, expand * 2f) { LineJoin = LineJoin.Round })
                    path.Widen(pen);
            }
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RebuildBitmaps();   // 尺寸变化时重建位图
            this.Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            RebuildBitmaps();
            this.Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isAlarming)
                AlarmClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // ★ 只做 DrawImage，零 GDI+ 路径计算
            if (_bmpOn == null) RebuildBitmaps();

            Bitmap bmp;
            if (!_isAlarming)
                bmp = _bmpIdle;
            else if (_isFlashOn)
                bmp = _bmpOn;
            else
                bmp = _bmpOff;

            if (bmp != null)
                e.Graphics.DrawImage(bmp, 0, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_blinkTimer != null)
                {
                    _blinkTimer.Stop();
                    _blinkTimer.Dispose();
                }
                _bmpOn?.Dispose();
                _bmpOff?.Dispose();
                _bmpIdle?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}


