using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Core
{
    public sealed class BufferNumerable
    {
        internal Dictionary<int, byte[]> Buffer = new Dictionary<int, byte[]>();
        public VarBuffer this[int index]
        {
            get
            {
                if (!Buffer.ContainsKey(index))
                    throw new IndexOutOfRangeException($"At the moment, the buffer has no active elements with the pointer '{index}'.");
                return VarBuffer.FromBinary(Buffer[index]);
            }
            set
            {
                if (Buffer.ContainsKey(index))
                    Buffer[index] = value.ToBinary();
                else
                    Buffer.Add(index, value.ToBinary());
            }
        }
    }
}
