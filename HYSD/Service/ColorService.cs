using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public class ColorService:IColorService
    {
        public Color ToColor(int argb)
        {
            return Color.FromArgb((argb & 0xFF0000) >> 16, (argb & 0xFF00) >> 8, argb & 0xFF);
        }
    }
}
