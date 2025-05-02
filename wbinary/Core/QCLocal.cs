using QuickC.Extensions;
using QuickC.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Core
{
    public class QCLocal
    {
        private TypeResolver _sr;
        public QCLocal()
        {
            _sr = new TypeResolver();
            _sr._qcConverter = this;
        }
        public QCLocal(TypeResolver typeResolver)
        {
            _sr = typeResolver;
            _sr._qcConverter = this;
        }
        public byte[] Serialize<T>(T obj, bool useCompression = true)
        {
            //step 1 - create container
            var container = new RawContainer();
            //step 2 - write headers
            container.Headers = new Headers
            {
                UseCompression = true,
                Major = Vnfo.Major,
                Minor = Vnfo.Minor,
            };
            //step 3 - write payload
            container.Payload = ConvertToBinary(obj).ToBinary();
            return container.ToBinary();
        }
        public async Task<byte[]> SerializeAsync<T>(T obj, bool useCompression = true)
        {
            return await Task.Run(() =>
            {
                return Serialize(obj, useCompression);
            });
        }
        public T? Deserialize<T>(byte[] source, bool ignoreVersion = false)
        {
            var container = RawContainer.FromBinary(source);
            if (!ignoreVersion && (container.Headers.Major > Vnfo.Major || container.Headers.Minor > Vnfo.Minor))
                throw new UnsupportedVersionException(container.Headers);
            return ConvertFromBinary<T>(BinaryVar.FromBinary(container.Payload));
        }
        public async Task<T?> DeserializeAsync<T>(byte[] source)
        {
            return await Task.Run(() =>
            {
                return Deserialize<T>(source);
            });
        }
        public BinaryVar ConvertToBinary(object? value)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    var buffer = new BinaryVar();
                    if (value == null)
                        buffer = buffer.SetHasValue(false);

                    else
                    {
                        buffer = buffer.SetHasValue(true);
                        writer.WriteResolve(value);
                    }
                    buffer = buffer.SetValue(m.ToArray());
                    return buffer;
                }
            }
        }
        public T? ConvertFromBinary<T>(BinaryVar buffer)
        {
            if (buffer.HasValue == false)
                return default(T?);
            using (MemoryStream m = new MemoryStream(buffer.Payload.ToArray()))
            {
                var type = typeof(T);
                using (BinaryReader reader = new BinaryReader(m))
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)reader.ReadBoolean();
                    }
                    if (typeof(T) == typeof(byte))
                    {
                        return (T)(object)reader.ReadByte();
                    }
                    if (typeof(T) == typeof(char))
                    {
                        return (T)(object)reader.ReadChar();
                    }
                    if (typeof(T) == typeof(decimal))
                    {
                        return (T)(object)reader.ReadDecimal();
                    }
                    if (typeof(T) == typeof(double))
                    {
                        return (T)(object)reader.ReadDouble();
                    }
                    if (typeof(T) == typeof(short))
                    {
                        return (T)(object)reader.ReadInt16();
                    }
                    if (typeof(T) == typeof(int))
                    {
                        return (T)(object)reader.ReadInt32();
                    }
                    if (typeof(T) == typeof(long))
                    {
                        return (T)(object)reader.ReadInt64();
                    }
                    if (typeof(T) == typeof(sbyte))
                    {
                        return (T)(object)reader.ReadSByte();
                    }
                    if (typeof(T) == typeof(float))
                    {
                        return (T)(object)reader.ReadSingle();
                    }
                    if (typeof(T) == typeof(ushort))
                    {
                        return (T)(object)reader.ReadUInt16();
                    }
                    if (typeof(T) == typeof(uint))
                    {
                        return (T)(object)reader.ReadUInt32();
                    }
                    if (typeof(T) == typeof(ulong))
                    {
                        return (T)(object)reader.ReadUInt64();
                    }
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)reader.ReadString();
                    }
                    if (typeof(T).FullName.StartsWith("System.Nullable"))
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(type);
                        return (T)ConvertFromBinary(buffer, underlyingType);
                    }
                    var resolver = _sr.FindReaderResolve(typeof(T));
                    if (resolver != null)
                        return (T)resolver?.Invoke(typeof(T), reader);
                    else
                    {
                        resolver = _sr.DefaultReaderResolve;
                        return (T)resolver?.Invoke(typeof(T), reader);
                    }
                }
            }
        }
        public object? ConvertFromBinary(BinaryVar buffer, Type type)
        {
            if (buffer.HasValue == false)
            {
                return null;
            }
            using (MemoryStream m = new MemoryStream(buffer.Payload.ToArray()))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    if (type == typeof(bool))
                    {
                        return reader.ReadBoolean();
                    }
                    if (type == typeof(byte))
                    {
                        return reader.ReadByte();
                    }
                    if (type == typeof(char))
                    {
                        return reader.ReadChar();
                    }
                    if (type == typeof(decimal))
                    {
                        return reader.ReadDecimal();
                    }
                    if (type == typeof(double))
                    {
                        return reader.ReadDouble();
                    }
                    if (type == typeof(short))
                    {
                        return reader.ReadInt16();
                    }
                    if (type == typeof(int))
                    {
                        return reader.ReadInt32();
                    }
                    if (type == typeof(long))
                    {
                        return reader.ReadInt64();
                    }
                    if (type == typeof(sbyte))
                    {
                        return reader.ReadSByte();
                    }
                    if (type == typeof(float))
                    {
                        return reader.ReadSingle();
                    }
                    if (type == typeof(ushort))
                    {
                        return reader.ReadUInt16();
                    }
                    if (type == typeof(uint))
                    {
                        return reader.ReadUInt32();
                    }
                    if (type == typeof(ulong))
                    {
                        return reader.ReadUInt64();
                    }
                    if (type == typeof(string))
                    {
                        return reader.ReadString();
                    }
                    if (type.FullName.StartsWith("System.Nullable"))
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(type);
                        return ConvertFromBinary(buffer, underlyingType);
                    }
                    else
                    {
                        var resolver = _sr.FindReaderResolve(type);
                        if (resolver != null)
                            return resolver?.Invoke(type, reader);
                        else
                        {
                            resolver = _sr.DefaultReaderResolve;
                            return resolver?.Invoke(type, reader);
                        }
                    }

                    throw new InvalidOperationException($"Unsupported type: {type}");
                }
            }
        }
    }
}
