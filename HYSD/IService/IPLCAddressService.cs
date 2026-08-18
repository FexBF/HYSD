using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public interface IPLCAddressService
    {
        DataSet ReadSheet();

        Dictionary<string, string> GetAddressMapping(DataSet dataSet,int index,string key,string value);
    }
}
