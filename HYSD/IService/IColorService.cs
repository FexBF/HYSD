using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public interface IColorService
    {
        Color ToColor(int argb);
    }
}
