using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncronizingBBJ
{
    class HiLoPrice
    {
        public DateTime BusinessDate { get; set; }
        public decimal CeilingPrice { get; set; }
        public decimal FloorPrice { get; set; }
    }
}
