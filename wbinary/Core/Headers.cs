using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Core
{
    public sealed class Headers
    {
        public bool UseCompression { get; set; } = true;
        internal byte Major { get; set; }
        internal byte Minor { get; set; }
    }
}
