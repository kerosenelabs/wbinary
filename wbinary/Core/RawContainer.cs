using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickC.Abstract;
using QuickC.Extensions;

namespace QuickC.Core
{
    internal class RawContainer : IRawContainer
    {
        public Headers Headers {  get; set; }
        public byte[] Payload { get; set; }

        public byte[] ToBinary()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter w = new BinaryWriter(m))
                {
                    //1 - byte
                    w.Write(Headers.Major);
                    //2 - byte
                    w.Write(Headers.Minor);
                    //3 - bool
                    w.Write(Headers.UseCompression);
                    //4 - bool
                    //w.Write(Headers.UseNodes);

                    //check is compression
                    if (Headers.UseCompression)
                        Payload = Payload.Zip();

                    //5 - int
                    w.Write(Payload.Length);
                    //6 - byte[]
                    w.Write(Payload);

                    return m.ToArray();
                }
            }
        }

        public static RawContainer FromBinary(byte[] raw)
        {
            var container = new RawContainer();
            container.Headers = new Headers();
            using (MemoryStream m = new MemoryStream(raw))
            {
                using (BinaryReader r = new BinaryReader(m))
                {
                    //1 - byte
                    container.Headers.Major = r.ReadByte();
                    //2 - byte
                    container.Headers.Minor = r.ReadByte();
                    //3 - bool
                    container.Headers.UseCompression = r.ReadBoolean();
                    //4 - bool
                    //container.Headers.UseNodes = r.ReadBoolean();
                    //5 - int
                    var length = r.ReadInt32();
                    //6 - byte[]
                    container.Payload = r.ReadBytes(length);

                    //check is compression
                    if(container.Headers.UseCompression)
                        container.Payload = container.Payload.Unzip();
                }
            }
            return container;
        }
    }
}
