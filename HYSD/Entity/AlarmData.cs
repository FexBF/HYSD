using HYSDControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HYSD
{
    public class AlarmData
    {

        public string Address { get; set; }

        public DateTime AlarmTime { get; set; }

        public string ErrText { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AlarmData other && Address == other.Address;
        }

        public override int GetHashCode()
        {
            return Address?.GetHashCode() ?? 0;
        }
    }
}
