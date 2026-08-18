using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HYSDControls
{
    public partial class MetalValve : UserControl
    {
        // 图片缓存字典
        private Dictionary<string, Image> _imageCache = new Dictionary<string, Image>();
        public MetalValve()
        {
            InitializeComponent();
            // 1. 加载原图 (假设你的资源里有一张水平泵的图)
            Image pumpHorizontal = Properties.Resources.阀门; // 替换为你的资源名称

            // 2. 将原图存入缓存
            _imageCache.Add("Pump_H", pumpHorizontal);

            // 🌟 3. 克隆原图，旋转90度，存入缓存 (垂直状态的泵)
            Image pumpVertical = (Image)pumpHorizontal.Clone(); // 必须Clone，否则会改掉原图
            pumpVertical.RotateFlip(RotateFlipType.Rotate90FlipNone); // 顺时针旋转90度
            _imageCache.Add("Pump_V", pumpVertical);
            this.pictureBox1.Image = _imageCache["Pump_H"];
           //设置控件样式
           this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        private bool isRotate = false;
        [Browsable(true)]
        [Category("自定义属性")]
        [Description("是否旋转")]
        public bool IsRotate
        {
            get { return isRotate; }
            set
            {
                isRotate = value;
                this.pictureBox1.Image = isRotate ? _imageCache["Pump_V"] : _imageCache["Pump_H"];
            }
        }
    }
}
