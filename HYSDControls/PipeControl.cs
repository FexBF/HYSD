using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YourNamespace // ← 替换为你的命名空间
{
    public enum PipeOrientation { Horizontal, Vertical }
    public enum FlowDirection { Forward, Reverse }

    public class PipeControl : Control
    {
        // ★ 修复：原代码每个 PipeControl 实例都 new 一个 Timer(40ms=25fps)，
        // 一个工艺页面常有十几个管道，就是十几个 25fps 定时器在疯狂 Invalidate，
        // GDI+ 重绘压力极大，是 UI 卡顿的重要原因。
        // 改为：全局唯一静态 Timer，只有 IsFlowing=true 的实例才注册到弱引用列表，
        // 每帧只推进这些实例的偏移并 Invalidate，其余管道零开销。
        private static readonly System.Collections.Generic.List<WeakReference<PipeControl>> _flowingInstances
            = new System.Collections.Generic.List<WeakReference<PipeControl>>();
        private static Timer _sharedTimer;
        private static int _registeredCount;

        private static void EnsureSharedTimer()
        {
            if (_sharedTimer != null) return;
            _sharedTimer = new Timer { Interval = 40 };
            _sharedTimer.Tick += (s, e) =>
            {
                if (_flowingInstances.Count == 0) return;
                // 倒序遍历，便于清理已失效的弱引用
                for (int i = _flowingInstances.Count - 1; i >= 0; i--)
                {
                    var wr = _flowingInstances[i];
                    PipeControl pc;
                    if (!wr.TryGetTarget(out pc) || pc == null || pc.IsDisposed)
                    {
                        _flowingInstances.RemoveAt(i);
                        _registeredCount--;
                        continue;
                    }
                    if (pc.IsFlowing)
                    {
                        pc._offset += pc.FlowSpeed;
                        if (pc._offset > 1000) pc._offset -= 1000;
                        try { pc.Invalidate(); } catch { }
                    }
                }
            };
            _sharedTimer.Start();
        }

        private void RegisterFlowing()
        {
            EnsureSharedTimer();
            _flowingInstances.Add(new WeakReference<PipeControl>(this));
            _registeredCount++;
            // 列表过长时做一次压缩清理（每 64 次注册清理一次失效项）
            if ((_registeredCount & 63) == 0)
            {
                _flowingInstances.RemoveAll(w =>
                {
                    PipeControl p;
                    return !w.TryGetTarget(out p) || p == null || p.IsDisposed;
                });
            }
        }

        private float _offset;

        #region 控件属性
        [Category("管道外观"), Description("管道摆放方向")]
        public PipeOrientation Orientation
        {
            get => _orientation;
            set { _orientation = value; Invalidate(); }
        }
        private PipeOrientation _orientation = PipeOrientation.Horizontal;

        [Category("管道流动"), Description("气流流动方向")]
        public FlowDirection Direction
        {
            get => _direction;
            set { _direction = value; Invalidate(); }
        }
        private FlowDirection _direction = FlowDirection.Forward;

        [Category("管道流动"), Description("是否正在抽真空")]
        public bool IsFlowing
        {
            get => _flowing;
            set
            {
                if (_flowing == value) return;
                _flowing = value;
                if (!value) _offset = 0;
                else RegisterFlowing(); // 首次流动时注册到共享定时器
                Invalidate();
            }
        }
        private bool _flowing = false;

        [Category("管道流动"), Description("气流速度(1-20，推荐6-12)")]
        public float FlowSpeed
        {
            get => _speed;
            set => _speed = Math.Max(1f, value);
        }
        private float _speed = 8f;

        [Category("管道颜色"), Description("管壁/法兰颜色")]
        public Color PipeColor
        {
            get => _pipeColor;
            set { _pipeColor = value; Invalidate(); }
        }
        private Color _pipeColor = Color.FromArgb(160, 160, 165);

        [Category("管道颜色"), Description("气流颜色(建议淡色)")]
        public Color FluidColor
        {
            get => _fluidColor;
            set { _fluidColor = value; Invalidate(); }
        }
        private Color _fluidColor = Color.FromArgb(100, 220, 255);

        [Category("管道颜色"), Description("管腔内部背景色")]
        public Color InnerColor
        {
            get => _innerColor;
            set { _innerColor = value; Invalidate(); }
        }
        private Color _innerColor = Color.FromArgb(20, 22, 28);

        [Category("管道尺寸"), Description("管道总粗细(支持小数微调，如14.5)")]
        public float PipeWidth
        {
            get => _width;
            set { _width = Math.Max(6f, value); Invalidate(); }
        }
        private float _width = 16f; // ★ 改为 float
        #endregion

        #region 初始化
        public PipeControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            // ★ 修复：不再为每个实例创建独立 Timer，改用静态共享 Timer（见类顶部）。
            // 只有 IsFlowing=true 时才会注册到共享定时器，零流动时零开销。

            Size = Orientation == PipeOrientation.Horizontal ? new Size(150, 30) : new Size(30, 150);
        }
        #endregion

        #region 主绘制逻辑
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // 🌟 开启高质量绘图，消除锯齿和马赛克
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;      // 让曲线和边缘平滑
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // 让图片缩放平滑
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; // 让文字清晰无锯齿
            g.Clear(BackColor);

            // ★ 全部基于 float 计算，完美支持小数微调
            float outerW = PipeWidth;
            float innerW = outerW * 0.7f;
            float flangeW = outerW * 1.4f;
            float flangeH = 2.5f;

            bool isH = (Orientation == PipeOrientation.Horizontal);
            float cx = Width / 2f;
            float cy = Height / 2f;

            // 1. 外管壁
            using (Pen outerPen = new Pen(PipeColor, outerW) { EndCap = LineCap.Flat, StartCap = LineCap.Flat })
            {
                if (isH) g.DrawLine(outerPen, 0, cy, Width, cy);
                else g.DrawLine(outerPen, cx, 0, cx, Height);
            }

            // 2. 负压空腔
            using (Pen innerPen = new Pen(InnerColor, innerW) { EndCap = LineCap.Flat, StartCap = LineCap.Flat })
            {
                if (isH) g.DrawLine(innerPen, 0, cy, Width, cy);
                else g.DrawLine(innerPen, cx, 0, cx, Height);
            }

            // 3. 裁剪区域
            float clipX = isH ? 0 : cx - innerW / 2;
            float clipY = isH ? cy - innerW / 2 : 0;
            float clipW = isH ? Width : innerW;
            float clipH = isH ? innerW : Height;
            g.SetClip(new RectangleF(clipX, clipY, clipW, clipH));

            // 4. 绘制气流
            if (IsFlowing)
            {
                DrawVacuumGas(g, innerW, isH, cx, cy);
            }

            g.ResetClip();

            // 5. 法兰
            using (Pen flangePen = new Pen(PipeColor, flangeH) { EndCap = LineCap.Flat, StartCap = LineCap.Flat })
            {
                if (isH)
                {
                    g.DrawLine(flangePen, 0, cy - flangeW / 2, 0, cy + flangeW / 2);
                    g.DrawLine(flangePen, Width, cy - flangeW / 2, Width, cy + flangeW / 2);
                }
                else
                {
                    g.DrawLine(flangePen, cx - flangeW / 2, 0, cx + flangeW / 2, 0);
                    g.DrawLine(flangePen, cx - flangeW / 2, Height, cx + flangeW / 2, Height);
                }
            }
        }
        #endregion

        #region 抽真空气流绘制
        private void DrawVacuumGas(Graphics g, float innerW, bool isH, float cx, float cy)
        {
            float pipeLen = isH ? Width : Height;

            int lineCount = 3;
            float lineSpacing = innerW / (lineCount + 1);

            using (Pen gasPen = new Pen(Color.FromArgb(55, FluidColor)) { EndCap = LineCap.Round })
            {
                for (int i = 1; i <= lineCount; i++)
                {
                    float offsetPos = i * lineSpacing;
                    float phaseOffset = i * 37.7f;
                    float currentOffset = (_offset + phaseOffset) % 120f;

                    float segLen = 18f + (i * 6f);
                    float totalCycle = segLen + 40f;

                    if (Direction == FlowDirection.Forward)
                    {
                        float startPos = -segLen - currentOffset;
                        for (float pos = startPos; pos < pipeLen; pos += totalCycle)
                        {
                            DrawGasLine(g, gasPen, isH, pos, offsetPos, cx, cy, segLen, innerW);
                        }
                    }
                    else
                    {
                        float startPos = pipeLen + currentOffset;
                        for (float pos = startPos; pos > -segLen; pos -= totalCycle)
                        {
                            DrawGasLine(g, gasPen, isH, pos, offsetPos, cx, cy, segLen, innerW);
                        }
                    }
                }
            }

            // 负压光晕
            using (Pen glowPen = new Pen(Color.FromArgb(12, FluidColor)) { EndCap = LineCap.Flat, StartCap = LineCap.Flat })
            {
                float glowW = innerW + 2f;
                if (isH) g.DrawLine(glowPen, 0, cy, Width, cy);
                else g.DrawLine(glowPen, cx, 0, cx, Height);
            }
        }

        private void DrawGasLine(Graphics g, Pen pen, bool isH, float pos, float offsetPos, float cx, float cy, float segLen, float innerW)
        {
            if (isH)
            {
                float y = cy - innerW / 2 + offsetPos;
                g.DrawLine(pen, pos, y, pos + segLen, y);
            }
            else
            {
                float x = cx - innerW / 2 + offsetPos;
                g.DrawLine(pen, x, pos, x, pos + segLen);
            }
        }
        #endregion

    }
}
