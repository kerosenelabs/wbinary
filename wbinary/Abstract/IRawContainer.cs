using QuickC.Core;
using QuickC.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Abstract
{
    public interface IRawContainer
    {
        Headers Headers { get; }
        byte[] Payload { get; }
        byte[] ToBinary();
        public static IRawContainer FromBinary(byte[] raw)
        {
            return RawContainer.FromBinary(raw);
        }
    }
}
