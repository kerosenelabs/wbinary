using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using wbinary.Abstract;
using wbinary.Extensions;

namespace wbinary.Core
{
    public static class QC
    {
        private const byte Major = 0;
        private const byte Minor = 13;
        private const byte pattern1 = 0x00;
        private const byte pattern2 = 0xFF;
        private const byte pattern3 = 0x00;
        private const byte pattern4 = 0xFF;
        private const byte pattern5 = 0x00;
        public static string VersionPrefix => $"{Major}.{Minor}";
        public static byte[] Serialize<T>(T obj, bool useCompression = true)
        {
            var bytes = ConvertToBinary(obj, 0).ToBinary();
            if (useCompression)
                bytes = bytes.Zip();
            return ConcatV(bytes, useCompression);
        }
        public static async Task<byte[]> SerializeAsync<T>(T obj)
        {
            return await Task.Run(() =>
            {
                return Serialize(obj);
            });
        }
        public static T? Deserialize<T>(byte[] source)
        {
            var vnfo = TryDecV(source);
            if (vnfo == null)
                return ConvertFromBinary<T>(VarBuffer.FromBinary(source.Unzip()));
            if(vnfo.Major > Major)
                throw new InvalidOperationException($"Requires QuickC version '{vnfo.Major}.{vnfo.Minor}', current version '{VersionPrefix}'");
            if (vnfo.Major == Major && vnfo.Minor > Minor)
                throw new InvalidOperationException($"Requires QuickC version '{vnfo.Major}.{vnfo.Minor}', current version '{VersionPrefix}'");
            source = TrimArray(source, 8, source.Length - 8);
            if(vnfo.IsCompressed)
                return ConvertFromBinary<T>(VarBuffer.FromBinary(source.Unzip()));
            else
                return ConvertFromBinary<T>(VarBuffer.FromBinary(source));
        }
        public static async Task<T?> DeserializeAsync<T>(byte[] source)
        {
            return await Task.Run(() =>
            {
                return Deserialize<T>(source);
            });
        }

        private static byte[] GenerateArrV(bool useCompr)
        {
            var vnfo = new Vnfo
            {
                Major = Major,
                Minor = Minor,
                IsCompressed = useCompr
            };
            var nfo = QC.ConvertToBinary(vnfo, -1).Source.ToArray();
            return nfo;
        }
        private static byte[] ConcatV(byte[] source, bool useCompr)
        {
            byte[] version = GenerateArrV(useCompr);
            var result = new byte[source.Length + version.Length + 5];
            result[0] = pattern1;
            result[1] = pattern2;
            result[2] = pattern3;
            result[3] = pattern4;
            result[4] = pattern5;
            version.CopyTo(result, 5);
            source.CopyTo(result, version.Length + 5);
            return result;
        }
        private static Vnfo TryDecV(byte[] raw)
        {
            if (raw[0] == pattern1 && raw[1] == pattern2 && raw[2] == pattern3 && raw[3] == pattern4 && raw[4] == pattern5)
            {
                byte[] v = new byte[3];
                v[0] = raw[5];
                v[1] = raw[6];
                v[2] = raw[7];
                var vnfo = QC.ConvertFromBinary<Vnfo>(new VarBuffer().SetPtr(-1).SetHasValue(true).SetValue(v));
                return vnfo;
            }
            else
                return null;
        }
        private static byte[] TrimArray(byte[] array, int startIndex, int length)
        {
            byte[] newArray = new byte[length];
            Array.Copy(array, startIndex, newArray, 0, length);
            return newArray;
        }

        public unsafe static VarBuffer ConvertToBinary(object value, int ptr)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    var buffer = new VarBuffer().SetPtr(ptr);
                    if (value == null)
                        buffer = buffer.SetHasValue(false);
                        
                    else
                    {
                        buffer = buffer.SetHasValue(true);
                        WriteValue(writer, value, true);
                    }
                    buffer = buffer.SetValue(m.ToArray());
                    return buffer;
                }
            }
        }
        public unsafe static T? ConvertFromBinary<T>(VarBuffer buffer)
        {
            if(buffer.HasValue == false)
                return default(T?);
            using (MemoryStream m = new MemoryStream(buffer.Source.ToArray()))
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
                    else
                    {
                        var resolver = TypeResolver.FindReaderResolve(typeof(T));
                        if (resolver != null)
                            return (T)resolver?.Invoke(typeof(T), reader);
                        else
                        {
                            resolver = TypeResolver.DefaultReaderResolve;
                            return (T)resolver?.Invoke(typeof(T), reader);
                        }
                    }

                    throw new InvalidOperationException($"Unsupported type: {typeof(T)}");
                }
            }
        }
        public unsafe static object? ConvertFromBinary(VarBuffer buffer, Type type)
        {
            if (buffer.HasValue == false)
            {
                return null;
            }
            using (MemoryStream m = new MemoryStream(buffer.Source.ToArray()))
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
                    if(type.FullName.StartsWith("System.Nullable"))
                    {
                        Type underlyingType = Nullable.GetUnderlyingType(type);
                        return ConvertFromBinary(buffer, underlyingType);
                    }
                    else
                    {
                        var resolver = TypeResolver.FindReaderResolve(type);
                        if (resolver != null)
                            return resolver?.Invoke(type, reader);
                        else
                        {
                            resolver = TypeResolver.DefaultReaderResolve;
                            return resolver?.Invoke(type, reader);
                        }
                    }

                    throw new InvalidOperationException($"Unsupported type: {type}");
                }
            }
        }

        internal static void WriteValue(BinaryWriter writer, object value, bool useResolverNative = false)
        {
            switch (value)
            {
                case bool boolValue:
                    writer.Write(boolValue);
                    break;
                case byte byteValue:
                    writer.Write(byteValue);
                    break;
                case char charValue:
                    writer.Write(charValue);
                    break;
                case decimal decimalValue:
                    writer.Write(decimalValue);
                    break;
                case double doubleValue:
                    writer.Write(doubleValue);
                    break;
                case short shortValue:
                    writer.Write(shortValue);
                    break;
                case int intValue:
                    writer.Write(intValue);
                    break;
                case long longValue:
                    writer.Write(longValue);
                    break;
                case sbyte sbyteValue:
                    writer.Write(sbyteValue);
                    break;
                case float floatValue:
                    writer.Write(floatValue);
                    break;
                case ushort ushortValue:
                    writer.Write(ushortValue);
                    break;
                case uint uintValue:
                    writer.Write(uintValue);
                    break;
                case ulong ulongValue:
                    writer.Write(ulongValue);
                    break;
                case string stringValue:
                    writer.Write(stringValue);
                    break;
                case Half HalfValue:
                    writer.Write(HalfValue);
                    break;
                default:
                    if (useResolverNative)
                    {
                        var resolver = TypeResolver.FindWriterResolve(value.GetType());
                        if(resolver != null)
                            resolver?.Invoke(value, writer);
                        else
                        {
                            resolver = TypeResolver.DefaultWriterResolve;
                            resolver?.Invoke(value, writer);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported type: {value.GetType()}");
                    }
                    break;
            }
        }
        internal static object ReadValue(BinaryReader reader, Type elementType, bool useResolveNative = false)
        {
            if (elementType == typeof(bool))
                return reader.ReadBoolean();
            if (elementType == typeof(byte))
                return reader.ReadByte();
            if (elementType == typeof(char))
                return reader.ReadChar();
            if (elementType == typeof(decimal))
                return reader.ReadDecimal();
            if (elementType == typeof(double))
                return reader.ReadDouble();
            if (elementType == typeof(short))
                return reader.ReadInt16();
            if (elementType == typeof(int))
                return reader.ReadInt32();
            if (elementType == typeof(long))
                return reader.ReadInt64();
            if (elementType == typeof(sbyte))
                return reader.ReadSByte();
            if (elementType == typeof(float))
                return reader.ReadSingle();
            if (elementType == typeof(ushort))
                return reader.ReadUInt16();
            if (elementType == typeof(uint))
                return reader.ReadUInt32();
            if (elementType == typeof(ulong))
                return reader.ReadUInt64();
            if (elementType == typeof(string))
                return reader.ReadString();
            if (useResolveNative)
            {
                var resolver = TypeResolver.FindReaderResolve(elementType);
                if (resolver != null)
                    return resolver?.Invoke(elementType, reader);
                else
                {
                    resolver = TypeResolver.DefaultReaderResolve;
                    return resolver?.Invoke(elementType, reader);
                }
            }

            throw new InvalidOperationException($"Unsupported array element type: {elementType}");
        }

        public static void WriteVarBuffer(VarBuffer buffer, BinaryWriter writer)
        {
            var binary = buffer.ToBinary();
            writer.Write(binary.Length);
            writer.Write(binary);
        }
        public static VarBuffer ReadVarBuffer(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var binary = reader.ReadBytes(length);
            return VarBuffer.FromBinary(binary);
        }

        public static class TypeResolver
        {
            private static List<ResolveObject> resolveObjects = new List<ResolveObject>();

            private static ResolveObject defaultResolveObject = new ResolveObject()
            {
                allowTypes = Array.Empty<Type>(),
                resolveTypeWrite = (obj, writer) =>
                {
                    var inspector = new ObjectInspector(obj);
                    var nodes = inspector.Inspect();
                    foreach (var node in nodes)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(node.Value, node.Ptr), writer);
                    }
                },
                resolveTypeRead = (type, reader) =>
                {
                    var nodes = ObjectInspector.Inspect(type);
                    var obj = type.CreateInstance();
                    foreach (var node in nodes)
                    {
                        var val = QC.ConvertFromBinary(QC.ReadVarBuffer(reader), node.ValueType);
                        node.SetValue(obj, val);
                    }
                    return obj;
                }
            };
            internal static ResolveTypeWrite DefaultWriterResolve => defaultResolveObject.resolveTypeWrite;
            internal static ResolveTypeRead DefaultReaderResolve => defaultResolveObject.resolveTypeRead;

            public static void RegisterResolve(ResolveTypeWrite writeResolve, ResolveTypeRead readResolve, params Type[] allowTypes)
            {
                resolveObjects.Add(new ResolveObject
                {
                    allowTypes = allowTypes,
                    resolveTypeWrite = writeResolve,
                    resolveTypeRead = readResolve,
                });
            }

            static TypeResolver()
            {
                //IBuffering
                RegisterResolve((obj, writer) =>
                {
                    var inspector = new ObjectInspector(obj);
                    var nodes = inspector.Inspect();
                    foreach (var node in nodes)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(node.Value, node.Ptr), writer);
                    }
                    //buffer of object
                    var buffering = obj as IBuffering;
                    var bf = new BufferNumerable();
                    buffering.WriteToBuffer(bf);
                    //count
                    writer.Write(bf.Buffer.Count);
                    foreach (var varBf in bf.Buffer)
                    {
                        writer.Write(varBf.Value.Length);
                        writer.Write(varBf.Key);
                        writer.Write(varBf.Value);
                    }
                },
                (type, reader) =>
                {
                    var nodes = ObjectInspector.Inspect(type);
                    var obj = type.CreateInstance();
                    foreach (var node in nodes)
                    {
                        var val = QC.ConvertFromBinary(QC.ReadVarBuffer(reader), node.ValueType);
                        node.SetValue(obj, val);
                    }

                    //buffer of object
                    var bf = new BufferNumerable();
                    var buffering = obj as IBuffering;
                    //count
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var length = reader.ReadInt32();
                        var key = reader.ReadInt32();
                        byte[] varBf = reader.ReadBytes(length);
                        bf[key] = VarBuffer.FromBinary(varBf);
                    }
                    buffering.ReadFromBuffer(bf);

                    return obj;
                },
                typeof(IBuffering));

                //Vnfo PRIVATE
                RegisterResolve((obj, writer) =>
                {
                    var nfo = obj as Vnfo;
                    writer.Write(nfo.Major);
                    writer.Write(nfo.Minor);
                    writer.Write(nfo.IsCompressed);
                },
                (type, reader) =>
                {
                    var obj = type.CreateInstance();
                    var nodes = ObjectInspector.Inspect(type);
                    nodes[0].SetValue(obj, reader.ReadByte());
                    nodes[1].SetValue(obj, reader.ReadByte());
                    nodes[2].SetValue(obj, reader.ReadBoolean());
                    return obj;
                },
                typeof(Vnfo));

                //Array
                RegisterResolve((obj, writer) =>
                {
                    var arr = obj as Array;
                    var length = arr.Length;
                    //0
                    writer.Write(length);
                    for (int i = 0; i < length; i++)
                    {
                        var buffer = QC.ConvertToBinary(arr.GetValue(i), i);
                        QC.WriteVarBuffer(buffer, writer);
                    }
                },
                (type, reader) =>
                {
                    Type elementType = type.GetElementType();
                    int length = reader.ReadInt32();
                    Array array = Array.CreateInstance(elementType, length);

                    for (int i = 0; i < length; i++)
                    {
                        var obj = QC.ConvertFromBinary(QC.ReadVarBuffer(reader), elementType);
                        array.SetValue(obj, i);
                    }
                    return array;
                },
                typeof(Array));

                //Dictionary
                RegisterResolve((obj, writer) =>
                {
                    var dictionary = obj as IDictionary;
                    writer.Write(dictionary.Count);

                    int ptr = 0;
                    foreach (var key in dictionary.Keys)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(key, ptr++), writer);
                    }
                    ptr = 0;
                    foreach (var val in dictionary.Values)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(val, ptr++), writer);
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();

                    var dictionary = type.CreateInstance();
                    var keyType = dictionary.GetType().GetGenericArguments()[0];
                    var valueType = dictionary.GetType().GetGenericArguments()[1];
                    var keys = Array.CreateInstance(keyType, length);
                    var values = Array.CreateInstance(valueType, length);

                    for (int i = 0; i < length; i++)
                    {
                        keys.SetValue(QC.ConvertFromBinary(QC.ReadVarBuffer(reader), keyType), i);
                    }

                    for (int i = 0; i < length; i++)
                    {
                        values.SetValue(QC.ConvertFromBinary(QC.ReadVarBuffer(reader), valueType), i);
                    }

                    for (int i = 0; i < length; i++)
                    {
                        dictionary.InvokeMethod("Add", keys.GetValue(i), values.GetValue(i));
                    }

                    return dictionary;
                },
                typeof(IDictionary));

                //IList
                RegisterResolve((obj, writer) =>
                {
                    var list = obj as IList;
                    writer.Write(list.Count);
                    int i = 0;
                    foreach (var item in list)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(item, i++), writer);
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var list = type.CreateInstance();
                    var itemType = list.GetType().GetGenericArguments()[0];
                    for (int i = 0; i < length; i++)
                    {
                        list.InvokeMethod("Add", QC.ConvertFromBinary(QC.ReadVarBuffer(reader), itemType));
                    }
                    return list;
                },
                typeof(IList));

                //DateTime
                RegisterResolve((obj, writer) =>
                {
                    var dt = (DateTime)obj;
                    writer.Write((long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds);
                },
                (type, reader) =>
                {
                    return DateTime.UnixEpoch.AddSeconds(reader.ReadInt64());
                },
                typeof(DateTime));

                //Enum
                RegisterResolve((obj, writer) =>
                {
                    object underlyingValue = Convert.ChangeType(obj, Enum.GetUnderlyingType(obj.GetType()));
                    WriteValue(writer, underlyingValue, true);
                },
                (type, reader) =>
                {
                    var underlyingType = Enum.GetUnderlyingType(type);
                    var obj = ReadValue(reader, underlyingType, true);
                    return obj;
                },
                typeof(Enum));

                //TimeSpan
                RegisterResolve((obj, writer) =>
                {
                    var ts = (TimeSpan)obj;
                    writer.Write(ts.TotalMilliseconds);
                },
                (type, reader) =>
                {
                    var tsRaw = reader.ReadDouble();
                    return TimeSpan.FromMilliseconds(tsRaw - 1);
                },
                typeof(TimeSpan));

                //DateTimeOffset
                RegisterResolve((obj, writer) =>
                {
                    var dto = (DateTimeOffset)obj;
                    var ticks = dto.Ticks;
                    var offset = dto.Offset.Ticks;
                    writer.Write(ticks);
                    writer.Write(offset);
                },
                (type, reader) =>
                {
                    var ticks = reader.ReadInt64();
                    var offset = reader.ReadInt64();
                    return new DateTimeOffset(ticks, new TimeSpan(offset));
                },
                typeof(DateTimeOffset));

                //BigInteger
                RegisterResolve((obj, writer) =>
                {
                    var bi = (BigInteger)obj;
                    var arr = bi.ToByteArray();
                    writer.Write(arr.Length);
                    writer.Write(arr);
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var arr = reader.ReadBytes(length);
                    return new BigInteger(arr);
                },
                typeof(BigInteger));

                //Complex
                RegisterResolve((obj, writer) =>
                {
                    var complex = (Complex)obj;
                    writer.Write(complex.Real);
                    writer.Write(complex.Imaginary);
                },
                (type, reader) =>
                {
                    var real = reader.ReadDouble();
                    var imaginary = reader.ReadDouble();
                    return new Complex(real, imaginary);
                },
                typeof(Complex));

                //Guid
                RegisterResolve((obj, writer) =>
                {
                    var guid = (Guid)obj;
                    var arr = guid.ToByteArray();
                    writer.Write(arr.Length);
                    writer.Write(arr);
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var arr = reader.ReadBytes(length);
                    return new Guid(arr);
                },
                typeof(Guid));

                //Uri
                RegisterResolve((obj, writer) =>
                {
                    var uri = (Uri)obj;
                    writer.Write(uri.AbsoluteUri);
                },
                (type, reader) =>
                {
                    return new Uri(reader.ReadString());
                },
                typeof(Uri));

                //Stack
                RegisterResolve((obj, writer) =>
                {
                    var pi = obj.GetType().GetProperty("Count");
                    var length = (int)pi.GetValue(obj);
                    var arr = Array.CreateInstance(obj.GetType().GetGenericArguments()[0], length);
                    obj.InvokeMethod("CopyTo", arr, 0);
                    Array.Reverse(arr);
                    writer.Write(length);
                    for(int i = 0; i < length; i++)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(arr.GetValue(i), i), writer);
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var itemType = type.GetGenericArguments()[0];
                    var obj = type.CreateInstance();
                    for (int i = 0; i < length; i++)
                    {
                        obj.InvokeMethod("Push", QC.ConvertFromBinary(QC.ReadVarBuffer(reader), itemType));
                    }
                    
                    return obj;
                },
                typeof(Stack<>));
            }

            internal static ResolveTypeWrite? FindWriterResolve(Type type)
            {
                var rObj = resolveObjects.FirstOrDefault(x => x.TypeContains(type));
                if (rObj == null)
                    return null;
                return rObj.resolveTypeWrite;
            }
            internal static ResolveTypeRead? FindReaderResolve(Type type)
            {
                var rObj = resolveObjects.FirstOrDefault(x => x.TypeContains(type));
                if (rObj == null)
                    return null;
                return rObj.resolveTypeRead;
            }
        }

        private class ResolveObject
        {
            public ResolveTypeWrite? resolveTypeWrite { get; set; }
            public ResolveTypeRead? resolveTypeRead { get; set; }
            public Type[] allowTypes {  get; set; }
            public bool TypeContains(Type type)
            {
                foreach (var x in allowTypes)
                {
                    
                    if(x.Equals(type))
                        return true;
                    else if (x.IsAssignableFrom(type))
                        return true;
                    else if (type.IsSubclassOf(x))
                        return true;
                    else if (type.IsGenericType && x.IsAssignableFrom(type.GetGenericTypeDefinition()))
                    {
                        return true;
                    }
                        
                }
                return false;
            }
        }
    }
    public delegate void ResolveTypeWrite(object obj, BinaryWriter writer);
    public delegate object ResolveTypeRead(Type type, BinaryReader reader);
}
