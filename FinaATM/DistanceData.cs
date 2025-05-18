using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinaATM
{
    public class DistanceData
    {
        public long IdFrom { get; set; }
        public string AddressFrom{ get; set; }
        public long IdTo { get; set; }
        public string AddressTo { get; set; }
        public double Distance { get; set; }
    }
}
