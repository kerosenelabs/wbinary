using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wbinary.Abstract;

namespace wbinary.Core
{
    internal class Vnfo
    {
        [Pointer(0)]
        public byte Major {  get; set; }
        [Pointer(1)]
        public byte Minor { get; set; }
        [Pointer(2)]
        public bool IsCompressed { get; set; }
    }
}
