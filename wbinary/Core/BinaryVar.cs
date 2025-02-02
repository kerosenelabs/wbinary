using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Core
{
    /// <summary>
    /// A container object for storing binary data, it takes up at least 1 bit for null, it usually takes up 1 bit and 4 bytes for a non-null object + Payload size
    /// </summary>
    public unsafe ref struct BinaryVar
    {
        public bool HasValue { get; internal set; }
        public Span<byte> Payload { get; internal set; }
        internal BinaryVar SetValue(Span<byte> value)
        {
            Payload = value;
            return this;
        }
        internal BinaryVar SetHasValue(bool isHasValue)
        {
            HasValue = isHasValue;
            return this;
        }

        public byte[] ToBinary()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(HasValue);
                    if (HasValue)
                    {
                        writer.Write(Payload.Length);
                        writer.Write(Payload);
                    }
                    return m.ToArray();
                }
            }
        }
        public static BinaryVar FromBinary(byte[] binary)
        {
            using (MemoryStream m = new MemoryStream(binary))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    var hasValue = reader.ReadBoolean();
                    if (hasValue)
                    {
                        var length = reader.ReadInt32();
                        var instance = reader.ReadBytes(length);
                        return new BinaryVar().SetHasValue(hasValue).SetValue(instance);
                    }
                    return new BinaryVar().SetHasValue(hasValue);
                }
            }
        }
    }
}
