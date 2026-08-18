using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HYSDControls
{
    public partial class SimpleLineControl : Control
    {
        private int _x1 = 0;
        private int _y1 = 0;
        private int _x2 = 100;
        private int _y2 = 0;
        private Color _lineColor = Color.Black;
        private float _lineWidth = 2f;

        [Category("布局")]
        public int X1 { get => _x1; set { _x1 = value; Invalidate(); } }

        [Category("布局")]
        public int Y1 { get => _y1; set { _y1 = value; Invalidate(); } }

        [Category("布局")]
        public int X2 { get => _x2; set { _x2 = value; Invalidate(); } }

        [Category("布局")]
        public int Y2 { get => _y2; set { _y2 = value; Invalidate(); } }

        [Category("外观")]
        public Color LineColor { get => _lineColor; set { _lineColor = value; Invalidate(); } }

        [Category("外观")]
        public float LineWidth { get => _lineWidth; set { _lineWidth = value; Invalidate(); } }

        public SimpleLineControl()
        {
            // 防闪烁 + 背景透明
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(100, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(_lineColor, _lineWidth))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; // 抗锯齿
                e.Graphics.DrawLine(pen, _x1, _y1, _x2, _y2);
            }
        }
    }
}
