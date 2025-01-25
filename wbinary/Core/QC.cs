using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private const byte Minor = 15;
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

                //IBufferingShort
                RegisterResolve((obj, writer) =>
                {
                    var inspector = new ObjectInspector(obj);
                    var nodes = inspector.Inspect();
                    foreach (var node in nodes)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(node.Value, node.Ptr), writer);
                    }
                    //buffer of object
                    var buffering = obj as IBufferingShort;
                    var bf = new BufferNumerableShort();
                    buffering.WriteToBuffer(bf);
                    //count
                    writer.Write(bf.Buffer.Count);
                    foreach (var varBf in bf.Buffer)
                    {
                        writer.Write(varBf.Length);
                        writer.Write(varBf);
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
                    var bf = new BufferNumerableShort();
                    var buffering = obj as IBufferingShort;
                    //count
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var length = reader.ReadInt32();
                        byte[] varBf = reader.ReadBytes(length);
                        bf[i] = VarBuffer.FromBinary(varBf);
                    }
                    buffering.ReadFromBuffer(bf);

                    return obj;
                },
                typeof(IBufferingShort));

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

                //List
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
                typeof(List<>));

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

                //Vector2
                RegisterResolve((obj, writer) =>
                {
                    var vec = (Vector2)obj;
                    writer.Write(vec.X);
                    writer.Write(vec.Y);
                },
                (type, reader) =>
                {
                    return new Vector2(reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Vector2));

                //Vector3
                RegisterResolve((obj, writer) =>
                {
                    var vec = (Vector3)obj;
                    writer.Write(vec.X);
                    writer.Write(vec.Y);
                    writer.Write(vec.Z);
                },
                (type, reader) =>
                {
                    return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Vector3));

                //Vector4
                RegisterResolve((obj, writer) =>
                {
                    var vec = (Vector4)obj;
                    writer.Write(vec.X);
                    writer.Write(vec.Y);
                    writer.Write(vec.Z);
                    writer.Write(vec.W);
                },
                (type, reader) =>
                {
                    return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Vector4));

                //Vector<>
                RegisterResolve((obj, writer) =>
                {
                    var itemType = obj.GetType().GetGenericArguments()[0];
                    var arr = Array.CreateInstance(itemType, (int)obj.GetType().GetProperty("Count").GetValue(obj));
                    var me = obj.GetType().GetMethods();
                    var cMe = me.FirstOrDefault(x => x.Name == "CopyTo" && typeof(Array).IsAssignableFrom(x.GetParameters()[0].ParameterType));
                    cMe.Invoke(obj, new[]{ arr });
                    writer.Write(arr.Length);
                    for(int i = 0; i < arr.Length; i++)
                    {
                        QC.WriteVarBuffer(QC.ConvertToBinary(arr.GetValue(i), i), writer);
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var itemType = type.GetGenericArguments()[0];
                    var arr = Array.CreateInstance(itemType, length);
                    for (int i = 0; i < length; i++)
                    {
                        arr.SetValue(QC.ConvertFromBinary(QC.ReadVarBuffer(reader), itemType), i);
                    }
                    return type.CreateInstanceWithArgs(arr);
                },
                typeof(Vector<>));

                //Matrix3x2
                RegisterResolve((obj, writer) =>
                {
                    var matx = (Matrix3x2)obj;
                    writer.Write(matx.M11);
                    writer.Write(matx.M12);
                    writer.Write(matx.M21);
                    writer.Write(matx.M22);
                    writer.Write(matx.M31);
                    writer.Write(matx.M32);
                },
                (type, reader) =>
                {
                    return new Matrix3x2(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Matrix3x2));

                //Matrix4x4
                RegisterResolve((obj, writer) =>
                {
                    var matx = (Matrix4x4)obj;
                    writer.Write(matx.M11);
                    writer.Write(matx.M12);
                    writer.Write(matx.M13);
                    writer.Write(matx.M14);
                    writer.Write(matx.M21);
                    writer.Write(matx.M22);
                    writer.Write(matx.M23);
                    writer.Write(matx.M24);
                    writer.Write(matx.M31);
                    writer.Write(matx.M32);
                    writer.Write(matx.M33);
                    writer.Write(matx.M34);
                    writer.Write(matx.M41);
                    writer.Write(matx.M42);
                    writer.Write(matx.M43);
                    writer.Write(matx.M44);
                },
                (type, reader) =>
                {
                    return new Matrix4x4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Matrix4x4));

                //Plane
                RegisterResolve((obj, writer) =>
                {
                    var plane = (Plane)obj;
                    WriteValue(writer, plane.Normal, true);
                    writer.Write(plane.D);
                },
                (type, reader) =>
                {
                    return new Plane((Vector3)ReadValue(reader, typeof(Vector3), true), reader.ReadSingle());
                },
                typeof(Plane));

                //Quaternion
                RegisterResolve((obj, writer) =>
                {
                    var q = (Quaternion)obj;
                    writer.Write(q.X);
                    writer.Write(q.Y);
                    writer.Write(q.Z);
                    writer.Write(q.W);
                },
                (type, reader) =>
                {
                    return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                },
                typeof(Quaternion));

                //TimeZoneInfo
                RegisterResolve((obj, writer) =>
                {
                    var q = (TimeZoneInfo)obj;
                    writer.Write(q.Id);
                    WriteValue(writer, q.BaseUtcOffset, true);
                    writer.Write(q.DisplayName);
                    writer.Write(q.StandardName);
                },
                (type, reader) =>
                {
                    return TimeZoneInfo.CreateCustomTimeZone(reader.ReadString(), (TimeSpan)ReadValue(reader, typeof(TimeSpan), true), reader.ReadString(), reader.ReadString());
                },
                typeof(TimeZoneInfo));

                //ValueTuple
                RegisterResolve((obj, writer) =>
                {
                    var intf = obj as ITuple;
                    var length = intf.Length;
                    writer.Write(length);
                    for(int i = 0; i < length; i++)
                    {
                        var value = intf[i];
                        QC.WriteVarBuffer(QC.ConvertToBinary(value, i), writer);
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var tuple = type.CreateInstance();
                    
                    var intf = (ITuple)tuple;
                    if(intf.Length != length)
                        throw new EndOfStreamException($"The tuple you are trying to deserialize has a length of '{length}' arguments, the requested type has '{intf.Length}' arguments.");

                    var types = new List<Type>();
                    var currentType = type;

                    while (currentType != null && currentType.IsGenericType)
                    {
                        var currentTypes = currentType.GetGenericArguments();
                        types.AddRange(currentTypes.Take(7));

                        currentType = currentTypes.Length == 8 ? currentTypes[7] : null;
                    }

                    for (int i = 0;i < length; i++)
                    {
                        var value = QC.ConvertFromBinary(QC.ReadVarBuffer(reader), types[i]);
                        SetValueTuple(tuple, value, i);
                    }
                    return tuple;
                },
                typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>), typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>));

                //Tuple
                RegisterResolve((obj, writer) =>
                {
                    var vt = CallToValueTuple(obj);
                    WriteValue(writer, vt, true);
                },
                (type, reader) =>
                {
                    var vttype = TupleToValueTupleType(type);
                    var vt = ReadValue(reader, vttype, true);
                    return CallToTuple(vt);
                },
                typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>));
            }

            private static void SetValueTuple(object tuple, object? value, int index)
            {
                var tupleType = tuple.GetType();
                if (index < 7)
                {
                    var field = tupleType.GetField($"Item{index + 1}");
                    field.SetValue(tuple, value);
                }
                else
                {
                    var rest = tupleType.GetGenericArguments()[7];
                    var restField = tupleType.GetField("Rest");
                    var restTuple = restField.GetValue(tuple);
                    var restItemField = rest.GetField($"Item{(index - 6)}");
                    restItemField.SetValue(restTuple, value);
                    restField.SetValue(tuple, restTuple);
                }
            }
            public static Type TupleToValueTupleType(Type tupleType)
            {
                if (!tupleType.IsGenericType || !tupleType.FullName.StartsWith("System.Tuple"))
                    throw new ArgumentException("Type must be a Tuple");

                var allTypes = new List<Type>();
                var currentType = tupleType;

                while (currentType != null && currentType.IsGenericType)
                {
                    var currentArgs = currentType.GetGenericArguments();
                    allTypes.AddRange(currentArgs.Take(7));
                    currentType = currentArgs.Length == 8 ? currentArgs[7] : null;
                }

                var types = allTypes.ToArray();
                var resultType = types.Length switch
                {
                    1 => typeof(ValueTuple<>).MakeGenericType(types),
                    2 => typeof(ValueTuple<,>).MakeGenericType(types),
                    3 => typeof(ValueTuple<,,>).MakeGenericType(types),
                    4 => typeof(ValueTuple<,,,>).MakeGenericType(types),
                    5 => typeof(ValueTuple<,,,,>).MakeGenericType(types),
                    6 => typeof(ValueTuple<,,,,,>).MakeGenericType(types),
                    7 => typeof(ValueTuple<,,,,,,>).MakeGenericType(types),
                    _ => CreateNestedValueTuple(types)
                };
                return resultType;
            }
            private static Type CreateNestedValueTuple(Type[] types)
            {
                var remaining = types.Skip(7).ToArray();
                var restType = remaining.Length == 1
                    ? typeof(ValueTuple<>).MakeGenericType(remaining)
                    : CreateNestedValueTuple(remaining);

                return typeof(ValueTuple<,,,,,,,>).MakeGenericType(
                    types[0], types[1], types[2], types[3],
                    types[4], types[5], types[6], restType);
            }
            public static object CallToValueTuple(object tuple)
            {
                var tupleExtensions = typeof(TupleExtensions);
                var tupleType = tuple.GetType();
                var genericArgs = tupleType.GetGenericArguments();

                var methodName = "ToValueTuple";
                var methods = tupleExtensions.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == methodName &&
                               m.GetGenericArguments().Length == genericArgs.Length)
                    .ToList();

                var method = methods.First();
                var genericMethod = method.MakeGenericMethod(genericArgs);
                return genericMethod.Invoke(null, new[] { tuple });
            }
            public static object CallToTuple(object valueTuple)
            {
                var tupleExtensions = typeof(TupleExtensions);
                var tupleType = valueTuple.GetType();
                var genericArgs = tupleType.GetGenericArguments();

                var methodName = "ToTuple";
                var methods = tupleExtensions.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == methodName &&
                               m.GetGenericArguments().Length == genericArgs.Length)
                    .ToList();

                var method = methods.First();
                var genericMethod = method.MakeGenericMethod(genericArgs);
                return genericMethod.Invoke(null, new[] { valueTuple });
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
