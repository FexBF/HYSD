using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YourNamespace // ← 请替换为你的实际命名空间
{
    public enum PumpState { Off, Running, Overload }
    public enum PipeDirection { Top, Bottom, Left, Right }

    public class PumpControl : Control
    {
        #region 私有字段
        //private Timer _timer;
        private float _angle;
        private bool _blink;
        #endregion

        #region 控件属性（会显示在属性面板中）
        [Category("泵状态"), Description("泵的当前运行状态")]
        public PumpState State
        {
            get => _state;
            set
            {
                _state = value;
                _angle = 0;
                _blink = true;
                // ★ 运行/过载才开 Timer，停止时关 Timer
                //if (_state == PumpState.Off) _timer.Stop();
                //else _timer.Start();
                Invalidate();
            }
        }
        private PumpState _state = PumpState.Off;

        [Category("泵动画"), Description("正常运行时的旋转速度(1-50)")]
        public float RunSpeed { get => _runSpeed; set => _runSpeed = Math.Max(1f, value); }
        private float _runSpeed = 15f;

        [Category("泵动画"), Description("过载时的旋转速度(1-20)")]
        public float OverloadSpeed { get => _olSpeed; set => _olSpeed = Math.Max(1f, value); }
        private float _olSpeed = 4f;

        [Category("泵外观"), Description("流体进口方向")]
        public PipeDirection InletDirection
        {
            get => _inlet;
            set
            {
                _inlet = value;
                if (_inlet == _outlet) _outlet = GetOpposite(_inlet); // 防止进出口同向
                Invalidate();
            }
        }
        private PipeDirection _inlet = PipeDirection.Bottom;

        [Category("泵外观"), Description("流体出口方向")]
        public PipeDirection OutletDirection
        {
            get => _outlet;
            set
            {
                _outlet = value;
                if (_inlet == _outlet) _inlet = GetOpposite(_outlet); // 防止进出口同向
                Invalidate();
            }
        }
        private PipeDirection _outlet = PipeDirection.Right;

        [Category("泵外观"), Description("运行状态颜色")]
        public Color ColorRunning { get; set; } = Color.LimeGreen;
        [Category("泵外观"), Description("停机状态颜色")]
        public Color ColorOff { get; set; } = Color.Gray;
        [Category("泵外观"), Description("过载报警颜色")]
        public Color ColorOverload { get; set; } = Color.Red;

        public override string Text { get => base.Text; set { base.Text = value; Invalidate(); } }
        #endregion

        #region 初始化
        public PumpControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            //_timer = new Timer { Interval = 60 }; // 25帧/秒
            //_timer.Tick += (s, e) =>
            //{
            //    if (State == PumpState.Running)
            //    {
            //        _angle = (_angle + _runSpeed) % 360;
            //        Invalidate();
            //    }
            //    else if (State == PumpState.Overload)
            //    {
            //        _angle = (_angle + _olSpeed) % 360;
            //        _blink = !_blink;
            //        Invalidate();
            //    }
            //};
            //_timer.Start();

            Size = new Size(120, 140);
            Font = new Font("微软雅黑", 9f);
        }
        #endregion

        #region 绘制逻辑
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // 🌟 开启高质量绘图，消除锯齿和马赛克
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;      // 让曲线和边缘平滑
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // 让图片缩放平滑
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; // 让文字清晰无锯齿
            g.Clear(BackColor);

            float cx = Width / 2f;
            float cy = (Height - 20f) / 2f; // 底部留20px给文字
            float r = Math.Min(Width, Height - 20f) / 2f - 15f; // 泵体半径
            float pipeW = r * 0.5f;
            float pipeL = r * 0.7f;

            // 1. 决定当前颜色
            Color curColor = ColorOff;
            if (State == PumpState.Running) curColor = ColorRunning;
            if (State == PumpState.Overload) curColor = _blink ? ColorOverload : ColorOff;

            // 2. 绘制进出口管道
            DrawPipe(g, cx, cy, r, pipeL, pipeW, InletDirection, curColor);
            DrawPipe(g, cx, cy, r, pipeL, pipeW, OutletDirection, curColor);

            // 3. 绘制泵体外圈
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, curColor)))
            using (Pen circlePen = new Pen(curColor, 3f))
            {
                g.FillEllipse(bgBrush, cx - r, cy - r, r * 2, r * 2);
                g.DrawEllipse(circlePen, cx - r, cy - r, r * 2, r * 2);
            }

            // 4. 绘制旋转叶轮
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_angle);

            using (Pen bladePen = new Pen(curColor, 3f) { EndCap = LineCap.Round })
            {
                for (int i = 0; i < 3; i++)
                {
                    g.RotateTransform(120f);
                    g.DrawLine(bladePen, 0, 0, 0, r * 0.7f);
                }
            }
            using (SolidBrush centerBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(centerBrush, -3, -3, 6, 6);
            }
            g.ResetTransform();

            // 5. 绘制文字
            if (!string.IsNullOrEmpty(Text))
            {
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near })
                {
                    g.DrawString(Text, Font, textBrush, cx, Height - 18f, sf);
                }
            }
        }

        // 绘制单根管道
        private void DrawPipe(Graphics g, float cx, float cy, float r, float len, float w, PipeDirection dir, Color color)
        {
            float x1 = cx, y1 = cy, x2 = cx, y2 = cy;
            switch (dir)
            {
                case PipeDirection.Top: y1 = cy - r; y2 = cy - r - len; break;
                case PipeDirection.Bottom: y1 = cy + r; y2 = cy + r + len; break;
                case PipeDirection.Left: x1 = cx - r; x2 = cx - r - len; break;
                case PipeDirection.Right: x1 = cx + r; x2 = cx + r + len; break;
            }

            using (Pen pipePen = new Pen(color, w) { StartCap = LineCap.Flat, EndCap = LineCap.Flat })
            {
                g.DrawLine(pipePen, x1, y1, x2, y2);
            }

            // 画管道端口小法兰
            float flangeW = w * 1.4f;
            float flangeH = 3f;
            bool isVertical = (dir == PipeDirection.Top || dir == PipeDirection.Bottom);

            using (Pen flangePen = new Pen(color, flangeH) { StartCap = LineCap.Flat, EndCap = LineCap.Flat })
            {
                if (isVertical)
                    g.DrawLine(flangePen, x1 - flangeW / 2, y1, x1 + flangeW / 2, y1);
                else
                    g.DrawLine(flangePen, x1, y1 - flangeW / 2, x1, y1 + flangeW / 2);
            }
        }
        #endregion

        #region 辅助方法
        private PipeDirection GetOpposite(PipeDirection dir)
        {
            return dir == PipeDirection.Top ? PipeDirection.Bottom :
                   dir == PipeDirection.Bottom ? PipeDirection.Top :
                   dir == PipeDirection.Left ? PipeDirection.Right : PipeDirection.Left;
        }
        #endregion

        //protected override void OnVisibleChanged(EventArgs e)
        //{
        //    base.OnVisibleChanged(e);
        //    if (!Visible || _state == PumpState.Off)
        //        _timer.Stop();
        //    else
        //        _timer.Start();
        //}
    }
}
