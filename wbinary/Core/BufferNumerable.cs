﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Core
{
    public sealed class BufferNumerable
    {
        private int _value = 0;
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
