using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Core
{
    public sealed class BufferNumerableShort
    {
        private int _value = 0;
        public int ReadIndex => _value;
        internal List<byte[]> Buffer = new List<byte[]>();
        public VarBuffer this[int index]
        {
            get
            {
                if (Buffer.ElementAtOrDefault(index) == null)
                    throw new IndexOutOfRangeException($"At the moment, the buffer has no active elements with the pointer '{index}'.");
                return VarBuffer.FromBinary(Buffer[index]);
            }
            set
            {
                if (index >= Buffer.Count)
                    Buffer.Add(value.ToBinary());
                else
                    Buffer[index] = value.ToBinary();
            }
        }

        public void WriteNext<T>(T obj)
        {
            var i = Buffer.Count;
            this[i] = QC.ConvertToBinary(obj, i);
        }

        public T? ReadNext<T>() 
        {
            return QC.ConvertFromBinary<T>(this[_value++]);
        }

        public void ResetReadIndex()
        {
            _value = 0;
        }
    }
}
