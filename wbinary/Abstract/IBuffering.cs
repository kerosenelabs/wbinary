using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wbinary.Core;

namespace wbinary.Abstract
{
    public interface IBuffering
    {
        void WriteToBuffer(BufferNumerable buffer);
        void ReadFromBuffer(BufferNumerable buffer);
    }
    public interface IBufferingShort
    {
        void WriteToBuffer(BufferNumerableShort buffer);
        void ReadFromBuffer(BufferNumerableShort buffer);
    }
}
