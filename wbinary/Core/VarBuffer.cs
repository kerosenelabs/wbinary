using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Core
{
    public unsafe ref struct VarBuffer
    {
        public int Ptr {  get; internal set; }
        public bool HasValue { get; internal set; }
        public Span<byte> Source { get; internal set; }
        internal VarBuffer SetValue(Span<byte> value)
        {
            Source = value;
            return this;
        }
        internal VarBuffer SetHasValue(bool isHasValue)
        {
            HasValue = isHasValue;
            return this;
        }
        internal VarBuffer SetPtr(int ptr)
        {
            Ptr = ptr;
            return this;
        }

        public byte[] ToBinary()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Source.Length);
                    writer.Write(Ptr);
                    writer.Write(HasValue);
                    writer.Write(Source);
                    return m.ToArray();
                }
            }
        }
        public static VarBuffer FromBinary(byte[] binary)
        {
            using (MemoryStream m = new MemoryStream(binary))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    var length = reader.ReadInt32();
                    var ptr = reader.ReadInt32();
                    var hasValue = reader.ReadBoolean();
                    var instance = reader.ReadBytes(length);
                    return new VarBuffer().SetPtr(ptr).SetHasValue(hasValue).SetValue(instance);
                }
            }
        }
    }
}
