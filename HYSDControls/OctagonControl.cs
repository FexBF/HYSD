using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OctagonControlDemo
{
    public class OctagonControl : Control
    {
        private Color _fillColor = Color.SteelBlue;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2f;
        private float _rotationAngle = 0f;

        private Color _rectColor = Color.Gray;
        private float _rectWidth = 20f;
        private Font _textFont = new Font("Arial", 10, FontStyle.Bold);
        private Color _textColor = Color.White;
        private int[] _edgeNumbers;

        public OctagonControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(220, 220);
           _edgeNumbers = new int[] { 2, 1, 6, 7, 5, 4, 3, 8 };
        }

        #region 外观属性
        [Category("Appearance")]
        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public float BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public float RotationAngle
        {
            get => _rotationAngle;
            set { _rotationAngle = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color RectangleColor
        {
            get => _rectColor;
            set { _rectColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public float RectangleWidth
        {
            get => _rectWidth;
            set { _rectWidth = Math.Max(2, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Font TextFont
        {
            get => _textFont;
            set { _textFont = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }
        #endregion

        #region 数字设置
        public void SetEdgeNumber(int edgeIndex, int number)
        {
            if (edgeIndex < 0 || edgeIndex >= 8) return;
            _edgeNumbers[edgeIndex] = number;
            Invalidate();
        }

        public void SetAllEdgeNumbers(int[] numbers)
        {
            if (numbers == null || numbers.Length != 8) return;
            Array.Copy(numbers, _edgeNumbers, 8);
            Invalidate();
        }

        // 可选：设计时可用的属性
        [Category("Data")]
        [Description("所有边的数字（长度为8）")]
        public int[] EdgeNumbers
        {
            get => _edgeNumbers;
            set
            {
                if (value != null && value.Length == 8)
                {
                    _edgeNumbers = value;
                    Invalidate();
                }
            }
        }
        #endregion

        // 计算正八边形顶点（预留边距，防止外侧矩形被裁剪）
        private PointF[] GetOctagonVertices()
        {
            float margin = _rectWidth + _borderWidth + 5;
            float availableWidth = Width - 2 * margin;
            float availableHeight = Height - 2 * margin;
            if (availableWidth <= 0 || availableHeight <= 0)
                return new PointF[0];

            float cx = Width / 2f;
            float cy = Height / 2f;
            float radius = Math.Min(availableWidth, availableHeight) / 2f;

            PointF[] pts = new PointF[8];
            double angleStep = 360.0 / 8;
            double startAngle = -67.5 + _rotationAngle; // 一条边朝上

            for (int i = 0; i < 8; i++)
            {
                double rad = (startAngle + i * angleStep) * Math.PI / 180.0;
                pts[i] = new PointF(cx + radius * (float)Math.Cos(rad),
                                    cy + radius * (float)Math.Sin(rad));
            }
            return pts;
        }

        private struct Vector2
        {
            public float X, Y;
            public Vector2(float x, float y) { X = x; Y = y; }
            public static Vector2 operator *(Vector2 v, float f) => new Vector2(v.X * f, v.Y * f);
            public static Vector2 operator /(Vector2 v, float f) => new Vector2(v.X / f, v.Y / f);
            public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
            public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
        }

        private (PointF A, PointF B, PointF Mid, float Length, Vector2 Normal, float EdgeAngle)[] GetEdgesInfo(PointF[] vertices)
        {
            if (vertices.Length != 8) return new (PointF, PointF, PointF, float, Vector2, float)[0];
            var edges = new (PointF, PointF, PointF, float, Vector2, float)[8];
            PointF center = new PointF(Width / 2f, Height / 2f);

            for (int i = 0; i < 8; i++)
            {
                PointF a = vertices[i];
                PointF b = vertices[(i + 1) % 8];
                PointF mid = new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                float dx = b.X - a.X;
                float dy = b.Y - a.Y;
                float length = (float)Math.Sqrt(dx * dx + dy * dy);
                Vector2 edgeDir = new Vector2(dx / length, dy / length);
                Vector2 normal = new Vector2(-edgeDir.Y, edgeDir.X);
                Vector2 toMid = new Vector2(mid.X - center.X, mid.Y - center.Y);
                if (Vector2.Dot(normal, toMid) < 0)
                    normal = new Vector2(edgeDir.Y, -edgeDir.X);
                float edgeAngle = (float)Math.Atan2(edgeDir.Y, edgeDir.X);
                edges[i] = (a, b, mid, length, normal, edgeAngle);
            }
            return edges;
        }

        private int GetTopEdgeIndex(PointF[] vertices)
        {
            var edges = GetEdgesInfo(vertices);
            if (edges.Length == 0) return -1;
            int topIdx = 0;
            float minY = edges[0].Mid.Y;
            for (int i = 1; i < edges.Length; i++)
            {
                if (edges[i].Mid.Y < minY)
                {
                    minY = edges[i].Mid.Y;
                    topIdx = i;
                }
            }
            return topIdx;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            PointF[] vertices = GetOctagonVertices();
            if (vertices.Length == 0) return;

            // 绘制八边形
            using (SolidBrush fillBrush = new SolidBrush(_fillColor))
                g.FillPolygon(fillBrush, vertices);
            if (_borderWidth > 0)
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPolygon(borderPen, vertices);
                }

            var edges = GetEdgesInfo(vertices);
            if (edges.Length == 0) return;

            int topEdge = GetTopEdgeIndex(vertices);
            bool skipTop = (topEdge >= 0 && topEdge < edges.Length);

            for (int i = 0; i < edges.Length; i++)
            {
                if (skipTop && i == topEdge) continue;

                var (A, B, mid, length, normal, edgeAngle) = edges[i];

                Vector2 edgeDir = new Vector2(B.X - A.X, B.Y - A.Y);
                edgeDir = edgeDir / length;
                Vector2 outward = normal;

                PointF rectCenter = new PointF(mid.X + outward.X * _rectWidth / 2,
                                               mid.Y + outward.Y * _rectWidth / 2);
                float halfLen = length / 2;
                float halfWid = _rectWidth / 2;

                PointF[] rectPoints = new PointF[4];
                rectPoints[0] = new PointF(rectCenter.X + edgeDir.X * halfLen + outward.X * halfWid,
                                           rectCenter.Y + edgeDir.Y * halfLen + outward.Y * halfWid);
                rectPoints[1] = new PointF(rectCenter.X - edgeDir.X * halfLen + outward.X * halfWid,
                                           rectCenter.Y - edgeDir.Y * halfLen + outward.Y * halfWid);
                rectPoints[2] = new PointF(rectCenter.X - edgeDir.X * halfLen - outward.X * halfWid,
                                           rectCenter.Y - edgeDir.Y * halfLen - outward.Y * halfWid);
                rectPoints[3] = new PointF(rectCenter.X + edgeDir.X * halfLen - outward.X * halfWid,
                                           rectCenter.Y + edgeDir.Y * halfLen - outward.Y * halfWid);

                using (SolidBrush rectBrush = new SolidBrush(_rectColor))
                    g.FillPolygon(rectBrush, rectPoints);
                using (Pen rectPen = new Pen(Color.Black, 1))
                    g.DrawPolygon(rectPen, rectPoints);

                // ★★★ 修改点：数字水平绘制，居中于矩形中心 ★★★
                string text = _edgeNumbers[i].ToString();
                SizeF textSize = g.MeasureString(text, _textFont);
                using (SolidBrush textBrush = new SolidBrush(_textColor))
                {
                    g.DrawString(text, _textFont, textBrush,
                                 rectCenter.X - textSize.Width / 2,
                                 rectCenter.Y - textSize.Height / 2);
                }
            }
        }
    }
}