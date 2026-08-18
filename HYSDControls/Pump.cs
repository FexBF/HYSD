using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HYSDControls
{
    /// <summary>
    /// 泵组合控件 - 由泵体（含涡轮）、联轴器、电机三部分组成
    /// 适用于 WinForms .NET Framework 4.8
    /// </summary>
    [ToolboxBitmap(typeof(Pump))]
    public class Pump : UserControl
    {
        #region 字段与默认值

        private Color _pumpBodyColor = Color.SteelBlue;
        private Color _motorBodyColor = Color.Gray;
        private Color _couplingColor = Color.DarkGray;
        private Color _turbineColor = Color.White;
        private Color _motorSymbolColor = Color.White;
        private Color _borderColor = Color.FromArgb(60, 60, 60);
        private Color _baseColor = Color.FromArgb(80, 80, 80);
        private Color _pipeColor = Color.FromArgb(100, 100, 100);
        private Color _labelColor = Color.White;
        private string _pumpLabelText = "泵";
        private string _motorLabelText = "电机";
        private float _borderWidth = 2f;
        private bool _showLabels = true;
        private bool _showBase = true;
        private bool _showPipes = true;
        private bool _isRunning = false;
        private int _animationAngle = 0;
        private Timer _animationTimer;

        #endregion

        #region 属性

        /// <summary>泵体颜色</summary>
        [Category("外观"), Description("泵体的填充颜色"), DefaultValue(typeof(Color), "SteelBlue")]
        public Color PumpBodyColor
        {
            get => _pumpBodyColor;
            set { _pumpBodyColor = value; Invalidate(); }
        }

        /// <summary>电机颜色</summary>
        [Category("外观"), Description("电机的填充颜色"), DefaultValue(typeof(Color), "Gray")]
        public Color MotorBodyColor
        {
            get => _motorBodyColor;
            set { _motorBodyColor = value; Invalidate(); }
        }

        /// <summary>联轴器颜色</summary>
        [Category("外观"), Description("联轴器的填充颜色"), DefaultValue(typeof(Color), "DarkGray")]
        public Color CouplingColor
        {
            get => _couplingColor;
            set { _couplingColor = value; Invalidate(); }
        }

        /// <summary>涡轮颜色</summary>
        [Category("外观"), Description("涡轮叶片的绘制颜色"), DefaultValue(typeof(Color), "White")]
        public Color TurbineColor
        {
            get => _turbineColor;
            set { _turbineColor = value; Invalidate(); }
        }

        /// <summary>电机符号颜色</summary>
        [Category("外观"), Description("电机内部符号的绘制颜色"), DefaultValue(typeof(Color), "White")]
        public Color MotorSymbolColor
        {
            get => _motorSymbolColor;
            set { _motorSymbolColor = value; Invalidate(); }
        }

        /// <summary>边框颜色</summary>
        [Category("外观"), Description("控件边框颜色"), DefaultValue(typeof(Color), "60,60,60")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>底座颜色</summary>
        [Category("外观"), Description("底座填充颜色"), DefaultValue(typeof(Color), "80,80,80")]
        public Color BaseColor
        {
            get => _baseColor;
            set { _baseColor = value; Invalidate(); }
        }

        /// <summary>管道颜色</summary>
        [Category("外观"), Description("进出口管道颜色"), DefaultValue(typeof(Color), "100,100,100")]
        public Color PipeColor
        {
            get => _pipeColor;
            set { _pipeColor = value; Invalidate(); }
        }

        /// <summary>标签文字颜色</summary>
        [Category("外观"), Description("标签文字颜色"), DefaultValue(typeof(Color), "White")]
        public Color LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; Invalidate(); }
        }

        /// <summary>泵标签文字</summary>
        [Category("外观"), Description("泵体上显示的标签文字"), DefaultValue("泵")]
        public string PumpLabelText
        {
            get => _pumpLabelText;
            set { _pumpLabelText = value; Invalidate(); }
        }

        /// <summary>电机标签文字</summary>
        [Category("外观"), Description("电机上显示的标签文字"), DefaultValue("电机")]
        public string MotorLabelText
        {
            get => _motorLabelText;
            set { _motorLabelText = value; Invalidate(); }
        }

        /// <summary>边框宽度</summary>
        [Category("外观"), Description("边框线宽"), DefaultValue(2f)]
        public float BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = Math.Max(0.5f, value); Invalidate(); }
        }

        /// <summary>是否显示标签</summary>
        [Category("外观"), Description("是否显示部件标签"), DefaultValue(true)]
        public bool ShowLabels
        {
            get => _showLabels;
            set { _showLabels = value; Invalidate(); }
        }

        /// <summary>是否显示底座</summary>
        [Category("外观"), Description("是否显示底座"), DefaultValue(true)]
        public bool ShowBase
        {
            get => _showBase;
            set { _showBase = value; Invalidate(); }
        }

        /// <summary>是否显示进出口管道</summary>
        [Category("外观"), Description("是否显示进出口管道"), DefaultValue(true)]
        public bool ShowPipes
        {
            get => _showPipes;
            set { _showPipes = value; Invalidate(); }
        }

        /// <summary>是否运行中（涡轮旋转动画）</summary>
        [Category("行为"), Description("是否处于运行状态（涡轮旋转动画）"), DefaultValue(false)]
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                if (_isRunning)
                    StartAnimation();
                else
                    StopAnimation();
            }
        }

        #endregion

        #region 构造函数

        public Pump()
        {
            this.Size = new Size(480, 260);
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            _animationTimer = new Timer();
            _animationTimer.Interval = 50;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        #endregion

        #region 动画

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationAngle = (_animationAngle + 15) % 360;
            Invalidate();
        }

        private void StartAnimation()
        {
            if (!_animationTimer.Enabled)
                _animationTimer.Start();
        }

        private void StopAnimation()
        {
            _animationTimer.Stop();
            _animationAngle = 0;
            Invalidate();
        }

        #endregion

        #region 绘制主入口

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = this.Width;
            int h = this.Height;

            // 计算各部件区域
            float padding = Math.Min(w, h) * 0.06f;
            float baseHeight = _showBase ? Math.Min(w, h) * 0.10f : 0;
            float bodyTop = padding;
            float bodyHeight = h - padding * 2 - baseHeight;

            // 泵体区域（左侧，占40%）
            float pumpWidth = (w - padding * 2) * 0.38f;
            float pumpX = padding;
            float pumpY = bodyTop;

            // 联轴器区域（中间，占12%）
            float couplingWidth = (w - padding * 2) * 0.12f;
            float couplingX = pumpX + pumpWidth;
            float couplingY = bodyTop + bodyHeight * 0.30f;
            float couplingHeight = bodyHeight * 0.40f;

            // 电机区域（右侧，占40%）
            float motorWidth = (w - padding * 2) * 0.38f;
            float motorX = couplingX + couplingWidth;
            float motorY = bodyTop;
            float motorHeight = bodyHeight;

            // 绘制底座
            if (_showBase)
            {
                DrawBase(g, padding, h - padding - baseHeight, w - padding * 2, baseHeight);
            }

            // 绘制进出口管道
            if (_showPipes)
            {
                DrawPipes(g, pumpX, pumpY, pumpWidth, bodyHeight, padding);
            }

            // 绘制泵体
            DrawPumpBody(g, pumpX, pumpY, pumpWidth, bodyHeight);

            // 绘制涡轮
            DrawTurbine(g, pumpX, pumpY, pumpWidth, bodyHeight);

            // 绘制联轴器
            DrawCoupling(g, couplingX, couplingY, couplingWidth, couplingHeight);

            // 绘制电机
            DrawMotor(g, motorX, motorY, motorWidth, motorHeight);

            // 绘制标签
            if (_showLabels)
            {
                DrawLabels(g, pumpX, pumpY, pumpWidth, bodyHeight, motorX, motorY, motorWidth, motorHeight);
            }
        }

        #endregion

        #region 绘制底座

        private void DrawBase(Graphics g, float x, float y, float width, float height)
        {
            // 底座主体
            using (SolidBrush brush = new SolidBrush(_baseColor))
            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                RectangleF rect = new RectangleF(x, y, width, height);
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

                // 底座装饰线
                float lineY1 = y + height * 0.3f;
                float lineY2 = y + height * 0.6f;
                using (Pen decoPen = new Pen(Color.FromArgb(120, Color.White), 1f))
                {
                    g.DrawLine(decoPen, x + 4, lineY1, x + width - 4, lineY1);
                    g.DrawLine(decoPen, x + 4, lineY2, x + width - 4, lineY2);
                }
            }
        }

        #endregion

        #region 绘制进出口管道

        private void DrawPipes(Graphics g, float pumpX, float pumpY, float pumpWidth, float pumpHeight, float padding)
        {
            float pipeWidth = Math.Max(8, pumpWidth * 0.14f);
            float pipeLength = pumpWidth * 0.35f;

            using (SolidBrush brush = new SolidBrush(_pipeColor))
            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                // 进口管道（左侧，水平）
                float inletY = pumpY + pumpHeight * 0.35f - pipeWidth / 2;
                RectangleF inletRect = new RectangleF(pumpX - pipeLength, inletY, pipeLength + 2, pipeWidth);
                g.FillRectangle(brush, inletRect);
                g.DrawRectangle(pen, inletRect.X, inletRect.Y, inletRect.Width, inletRect.Height);

                // 出口管道（顶部，垂直向上）
                float outletX = pumpX + pumpWidth * 0.5f - pipeWidth / 2;
                float outletLength = pumpHeight * 0.30f;
                RectangleF outletRect = new RectangleF(outletX, pumpY - outletLength, pipeWidth, outletLength + 2);
                g.FillRectangle(brush, outletRect);
                g.DrawRectangle(pen, outletRect.X, outletRect.Y, outletRect.Width, outletRect.Height);

                // 管道法兰（进口）
                float flangeSize = pipeWidth + 6;
                RectangleF inletFlange = new RectangleF(pumpX - pipeLength - 3, inletY - 3, flangeSize, flangeSize);
                g.FillRectangle(brush, inletFlange);
                g.DrawRectangle(pen, inletFlange.X, inletFlange.Y, inletFlange.Width, inletFlange.Height);

                // 管道法兰（出口）
                RectangleF outletFlange = new RectangleF(outletX - 3, pumpY - outletLength - 3, flangeSize, flangeSize);
                g.FillRectangle(brush, outletFlange);
                g.DrawRectangle(pen, outletFlange.X, outletFlange.Y, outletFlange.Width, outletFlange.Height);

                // 流向箭头 - 进口
                DrawFlowArrow(g, pumpX - pipeLength * 0.7f, inletY + pipeWidth / 2, true);
                // 流向箭头 - 出口
                DrawFlowArrow(g, outletX + pipeWidth / 2, pumpY - outletLength * 0.6f, false);
            }
        }

        private void DrawFlowArrow(Graphics g, float cx, float cy, bool horizontal)
        {
            using (SolidBrush arrowBrush = new SolidBrush(Color.FromArgb(200, Color.LightBlue)))
            {
                PointF[] arrowPts;
                float s = 6;
                if (horizontal)
                {
                    arrowPts = new PointF[]
                    {
                        new PointF(cx + s, cy),
                        new PointF(cx - s * 0.5f, cy - s * 0.6f),
                        new PointF(cx - s * 0.5f, cy + s * 0.6f)
                    };
                }
                else
                {
                    arrowPts = new PointF[]
                    {
                        new PointF(cx, cy - s),
                        new PointF(cx - s * 0.6f, cy + s * 0.5f),
                        new PointF(cx + s * 0.6f, cy + s * 0.5f)
                    };
                }
                g.FillPolygon(arrowBrush, arrowPts);
            }
        }

        #endregion

        #region 绘制泵体

        private void DrawPumpBody(Graphics g, float x, float y, float width, float height)
        {
            // 泵体圆角矩形
            float radius = Math.Min(width, height) * 0.08f;
            GraphicsPath path = CreateRoundedRectPath(x, y, width, height, radius);

            // 渐变填充
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x + width, y + height),
                Color.FromArgb(220, _pumpBodyColor), _pumpBodyColor))
            {
                g.FillPath(brush, path);
            }

            // 高光效果
            using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + height * 0.4f),
                Color.FromArgb(60, Color.White), Color.FromArgb(0, Color.White)))
            {
                GraphicsPath highlightPath = CreateRoundedRectPath(x + 2, y + 2, width - 4, height * 0.4f, radius);
                g.FillPath(highlightBrush, highlightPath);
            }

            // 边框
            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                g.DrawPath(pen, path);
            }

            // 泵体装饰 - 螺栓
            DrawBolts(g, x, y, width, height, 4);
        }

        #endregion

        #region 绘制涡轮

        private void DrawTurbine(Graphics g, float pumpX, float pumpY, float pumpWidth, float pumpHeight)
        {
            float cx = pumpX + pumpWidth * 0.5f;
            float cy = pumpY + pumpHeight * 0.5f;
            float turbineRadius = Math.Min(pumpWidth, pumpHeight) * 0.30f;

            // 涡轮外圈
            using (Pen outerPen = new Pen(Color.FromArgb(180, _turbineColor), 2f))
            {
                g.DrawEllipse(outerPen, cx - turbineRadius, cy - turbineRadius, turbineRadius * 2, turbineRadius * 2);
            }

            // 涡轮内圈
            float innerRadius = turbineRadius * 0.25f;
            using (SolidBrush centerBrush = new SolidBrush(Color.FromArgb(200, _turbineColor)))
            using (Pen centerPen = new Pen(_borderColor, 1.5f))
            {
                g.FillEllipse(centerBrush, cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2);
                g.DrawEllipse(centerPen, cx - innerRadius, cy - innerRadius, innerRadius * 2, innerRadius * 2);
            }

            // 涡轮叶片（6片，带旋转动画）
            int bladeCount = 6;
            float angleOffset = _isRunning ? _animationAngle : 0;

            using (GraphicsPath bladePath = new GraphicsPath())
            {
                for (int i = 0; i < bladeCount; i++)
                {
                    float angle = (360f / bladeCount) * i + angleOffset;
                    float rad = angle * (float)Math.PI / 180f;

                    // 叶片起点（中心附近）
                    float startR = innerRadius * 1.2f;
                    float sx = cx + startR * (float)Math.Cos(rad);
                    float sy = cy + startR * (float)Math.Sin(rad);

                    // 叶片终点（外圈附近）
                    float endR = turbineRadius * 0.88f;
                    float ex = cx + endR * (float)Math.Cos(rad);
                    float ey = cy + endR * (float)Math.Sin(rad);

                    // 叶片宽度方向（垂直于径向）
                    float perpAngle = rad + (float)Math.PI / 2f;
                    float bladeWidth = turbineRadius * 0.18f;

                    // 叶片弧线控制点 - 使叶片呈弧形
                    float midR = (startR + endR) * 0.55f;
                    float curveOffset = bladeWidth * 0.6f;
                    float cmx = cx + midR * (float)Math.Cos(rad) + curveOffset * (float)Math.Cos(perpAngle);
                    float cmy = cy + midR * (float)Math.Sin(rad) + curveOffset * (float)Math.Sin(perpAngle);

                    // 构建叶片形状（带弧度的四边形）
                    PointF[] bladePoints = new PointF[4];
                    float halfW = bladeWidth * 0.5f;

                    bladePoints[0] = new PointF(
                        sx + halfW * 0.3f * (float)Math.Cos(perpAngle),
                        sy + halfW * 0.3f * (float)Math.Sin(perpAngle));
                    bladePoints[1] = new PointF(
                        sx - halfW * 0.3f * (float)Math.Cos(perpAngle),
                        sy - halfW * 0.3f * (float)Math.Sin(perpAngle));
                    bladePoints[2] = new PointF(
                        ex - halfW * (float)Math.Cos(perpAngle),
                        ey - halfW * (float)Math.Sin(perpAngle));
                    bladePoints[3] = new PointF(
                        ex + halfW * (float)Math.Cos(perpAngle),
                        ey + halfW * (float)Math.Sin(perpAngle));

                    bladePath.Reset();
                    bladePath.AddCurve(new PointF[] { bladePoints[0],
                        new PointF(cmx + halfW * (float)Math.Cos(perpAngle), cmy + halfW * (float)Math.Sin(perpAngle)),
                        bladePoints[3] });
                    bladePath.AddLine(bladePoints[3], bladePoints[2]);
                    bladePath.AddCurve(new PointF[] { bladePoints[2],
                        new PointF(cmx - halfW * (float)Math.Cos(perpAngle), cmy - halfW * (float)Math.Sin(perpAngle)),
                        bladePoints[1] });
                    bladePath.CloseFigure();

                    using (SolidBrush bladeBrush = new SolidBrush(Color.FromArgb(160, _turbineColor)))
                    using (Pen bladePen = new Pen(Color.FromArgb(100, _borderColor), 1f))
                    {
                        g.FillPath(bladeBrush, bladePath);
                        g.DrawPath(bladePen, bladePath);
                    }
                }
            }

            // 涡轮中心点
            using (SolidBrush dotBrush = new SolidBrush(_borderColor))
            {
                float dotR = innerRadius * 0.4f;
                g.FillEllipse(dotBrush, cx - dotR, cy - dotR, dotR * 2, dotR * 2);
            }
        }

        #endregion

        #region 绘制联轴器

        private void DrawCoupling(Graphics g, float x, float y, float width, float height)
        {
            // 联轴器主体
            float radius = Math.Min(width, height) * 0.15f;
            GraphicsPath path = CreateRoundedRectPath(x, y, width, height, radius);

            // 渐变填充
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + height),
                Color.FromArgb(230, _couplingColor), _couplingColor))
            {
                g.FillPath(brush, path);
            }

            // 边框
            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                g.DrawPath(pen, path);
            }

            // 联轴器装饰 - 中心线
            float centerX = x + width / 2;
            using (Pen linePen = new Pen(Color.FromArgb(100, Color.Black), 1f))
            {
                g.DrawLine(linePen, centerX, y + 4, centerX, y + height - 4);
            }

            // 联轴器螺栓
            float boltR = Math.Min(width, height) * 0.06f;
            float boltY1 = y + height * 0.25f;
            float boltY2 = y + height * 0.75f;
            using (SolidBrush boltBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            {
                g.FillEllipse(boltBrush, centerX - boltR, boltY1 - boltR, boltR * 2, boltR * 2);
                g.FillEllipse(boltBrush, centerX - boltR, boltY2 - boltR, boltR * 2, boltR * 2);
            }
        }

        #endregion

        #region 绘制电机

        private void DrawMotor(Graphics g, float x, float y, float width, float height)
        {
            // 电机主体圆角矩形
            float radius = Math.Min(width, height) * 0.08f;
            GraphicsPath path = CreateRoundedRectPath(x, y, width, height, radius);

            // 渐变填充
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x + width, y + height),
                Color.FromArgb(220, _motorBodyColor), _motorBodyColor))
            {
                g.FillPath(brush, path);
            }

            // 高光效果
            using (LinearGradientBrush highlightBrush = new LinearGradientBrush(
                new PointF(x, y), new PointF(x, y + height * 0.35f),
                Color.FromArgb(50, Color.White), Color.FromArgb(0, Color.White)))
            {
                GraphicsPath highlightPath = CreateRoundedRectPath(x + 2, y + 2, width - 4, height * 0.35f, radius);
                g.FillPath(highlightBrush, highlightPath);
            }

            // 边框
            using (Pen pen = new Pen(_borderColor, _borderWidth))
            {
                g.DrawPath(pen, path);
            }

            // 电机内部符号
            DrawMotorSymbol(g, x, y, width, height);

            // 电机散热片装饰
            DrawMotorFins(g, x, y, width, height);

            // 电机螺栓
            DrawBolts(g, x, y, width, height, 4);

            // 接线盒（顶部小矩形）
            float boxWidth = width * 0.30f;
            float boxHeight = height * 0.15f;
            float boxX = x + width * 0.5f - boxWidth * 0.5f;
            float boxY = y - boxHeight * 0.5f;
            using (SolidBrush boxBrush = new SolidBrush(Color.FromArgb(180, _motorBodyColor)))
            using (Pen boxPen = new Pen(_borderColor, _borderWidth * 0.8f))
            {
                g.FillRectangle(boxBrush, boxX, boxY, boxWidth, boxHeight);
                g.DrawRectangle(boxPen, boxX, boxY, boxWidth, boxHeight);
            }
        }

        private void DrawMotorSymbol(Graphics g, float x, float y, float width, float height)
        {
            float cx = x + width * 0.5f;
            float cy = y + height * 0.5f;
            float symbolRadius = Math.Min(width, height) * 0.22f;

            // 电机圆形符号
            using (Pen symbolPen = new Pen(_motorSymbolColor, 2.5f))
            {
                g.DrawEllipse(symbolPen, cx - symbolRadius, cy - symbolRadius, symbolRadius * 2, symbolRadius * 2);
            }

            // "M" 字母
            using (Font mFont = new Font("Arial", symbolRadius * 0.9f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(_motorSymbolColor))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString("M", mFont, textBrush, cx, cy, sf);
            }

            // 旋转方向箭头（运行时显示）
            if (_isRunning)
            {
                float arrowRadius = symbolRadius * 1.25f;
                using (Pen arrowPen = new Pen(Color.FromArgb(180, _motorSymbolColor), 2f))
                {
                    // 绘制弧形箭头
                    float startAngle = -30f + (_animationAngle * 0.5f) % 360f;
                    float sweepAngle = 270f;
                    g.DrawArc(arrowPen, cx - arrowRadius, cy - arrowRadius, arrowRadius * 2, arrowRadius * 2, startAngle, sweepAngle);

                    // 箭头头部
                    float endAngleRad = (startAngle + sweepAngle) * (float)Math.PI / 180f;
                    float arrowX = cx + arrowRadius * (float)Math.Cos(endAngleRad);
                    float arrowY = cy + arrowRadius * (float)Math.Sin(endAngleRad);

                    float perpAngle = endAngleRad + (float)Math.PI / 2f;
                    float arrowSize = 5f;
                    PointF[] arrowHead = new PointF[]
                    {
                        new PointF(arrowX, arrowY),
                        new PointF(arrowX + arrowSize * (float)Math.Cos(perpAngle + 0.5f),
                                   arrowY + arrowSize * (float)Math.Sin(perpAngle + 0.5f)),
                        new PointF(arrowX + arrowSize * (float)Math.Cos(perpAngle - 0.5f),
                                   arrowY + arrowSize * (float)Math.Sin(perpAngle - 0.5f))
                    };
                    using (SolidBrush arrowBrush = new SolidBrush(Color.FromArgb(180, _motorSymbolColor)))
                    {
                        g.FillPolygon(arrowBrush, arrowHead);
                    }
                }
            }
        }

        private void DrawMotorFins(Graphics g, float x, float y, float width, float height)
        {
            // 右侧散热片
            int finCount = 5;
            float finSpacing = height * 0.7f / (finCount + 1);
            float finStartY = y + height * 0.15f;
            float finWidth = width * 0.08f;
            float finX = x + width - finWidth - 3;

            using (Pen finPen = new Pen(Color.FromArgb(80, Color.Black), 1.5f))
            {
                for (int i = 0; i < finCount; i++)
                {
                    float fy = finStartY + finSpacing * (i + 1);
                    g.DrawLine(finPen, finX, fy, finX + finWidth, fy);
                }
            }
        }

        #endregion

        #region 绘制螺栓

        private void DrawBolts(Graphics g, float x, float y, float width, float height, int count)
        {
            float boltR = Math.Max(2, Math.Min(width, height) * 0.025f);
            float inset = boltR * 2.5f;

            PointF[] positions = new PointF[]
            {
                new PointF(x + inset, y + inset),
                new PointF(x + width - inset, y + inset),
                new PointF(x + inset, y + height - inset),
                new PointF(x + width - inset, y + height - inset)
            };

            using (SolidBrush boltBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
            using (SolidBrush boltHighlight = new SolidBrush(Color.FromArgb(140, 140, 140)))
            {
                for (int i = 0; i < Math.Min(count, positions.Length); i++)
                {
                    g.FillEllipse(boltBrush, positions[i].X - boltR, positions[i].Y - boltR, boltR * 2, boltR * 2);
                    g.FillEllipse(boltHighlight, positions[i].X - boltR * 0.5f, positions[i].Y - boltR * 0.5f, boltR, boltR);
                }
            }
        }

        #endregion

        #region 绘制标签

        private void DrawLabels(Graphics g, float pumpX, float pumpY, float pumpWidth, float pumpHeight,
            float motorX, float motorY, float motorWidth, float motorHeight)
        {
            float fontSize = Math.Max(9, Math.Min(pumpWidth, pumpHeight) * 0.10f);
            using (Font labelFont = new Font("Microsoft YaHei", fontSize, FontStyle.Bold))
            using (SolidBrush labelBrush = new SolidBrush(_labelColor))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
            {
                // 泵标签
                g.DrawString(_pumpLabelText, labelFont, labelBrush,
                    pumpX + pumpWidth * 0.5f, pumpY + pumpHeight - 6, sf);

                // 电机标签
                g.DrawString(_motorLabelText, labelFont, labelBrush,
                    motorX + motorWidth * 0.5f, motorY + motorHeight - 6, sf);
            }
        }

        #endregion

        #region 辅助方法 - 圆角矩形路径

        private GraphicsPath CreateRoundedRectPath(float x, float y, float width, float height, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r = Math.Min(radius, Math.Min(width, height) / 2f);

            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion

        #region 清理

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateAnimationTimerState();
        }

        private void UpdateAnimationTimerState()
        {
            bool shouldRun = Visible && _isRunning;
            if (shouldRun && !_animationTimer.Enabled)
                _animationTimer.Start();
            else if (!shouldRun && _animationTimer.Enabled)
                _animationTimer.Stop();
        }
    }
}
