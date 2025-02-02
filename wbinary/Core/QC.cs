using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using QuickC.Abstract;
using QuickC.Extensions;

namespace QuickC.Core
{
    public static class QC
    {
        public static string VersionPrefix => $"{Vnfo.Major}.{Vnfo.Minor}";
        public static byte[] Serialize<T>(T obj, bool useCompression = true)
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
        public static async Task<byte[]> SerializeAsync<T>(T obj, bool useCompression = true)
        {
            return await Task.Run(() =>
            {
                return Serialize(obj, useCompression);
            });
        }
        public static T? Deserialize<T>(byte[] source, bool ignoreVersion = false)
        {
            var container = RawContainer.FromBinary(source);
            if (!ignoreVersion && (container.Headers.Major > Vnfo.Major || container.Headers.Minor > Vnfo.Minor))
                throw new UnsupportedVersionException(container.Headers);
            return ConvertFromBinary<T>(BinaryVar.FromBinary(container.Payload));
        }
        public static async Task<T?> DeserializeAsync<T>(byte[] source)
        {
            return await Task.Run(() =>
            {
                return Deserialize<T>(source);
            });
        }
        public static BinaryVar ConvertToBinary(object? value)
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
        public static T? ConvertFromBinary<T>(BinaryVar buffer)
        {
            if(buffer.HasValue == false)
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
                    var resolver = TypeResolver.FindReaderResolve(typeof(T));
                    if (resolver != null)
                        return (T)resolver?.Invoke(typeof(T), reader);
                    else
                    {
                        resolver = TypeResolver.DefaultReaderResolve;
                        return (T)resolver?.Invoke(typeof(T), reader);
                    }
                }
            }
        }
        public static object? ConvertFromBinary(BinaryVar buffer, Type type)
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
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(node.Value));
                    }
                },
                resolveTypeRead = (type, reader) =>
                {
                    var nodes = ObjectInspector.Inspect(type);
                    var obj = type.CreateInstance();
                    foreach (var node in nodes)
                    {
                        var val = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), node.ObjectType);
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
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(node.Value));
                    }
                    //buffer of object
                    var buffering = obj as IBuffering;
                    var bf = new ObjectBuffer();
                    buffering.OnWriteToBuffer(bf);
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
                        var val = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), node.ObjectType);
                        node.SetValue(obj, val);
                    }

                    //buffer of object
                    var bf = new ObjectBuffer();
                    var buffering = obj as IBuffering;
                    //count
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var length = reader.ReadInt32();
                        byte[] varBf = reader.ReadBytes(length);
                        bf[i] = BinaryVar.FromBinary(varBf);
                    }
                    buffering.OnReadFromBuffer(bf);

                    return obj;
                },
                typeof(IBuffering));
                #region IEnumerable<T>, ICollection<T>

                //Array
                RegisterResolve((obj, writer) =>
                {
                    var arr = obj as Array;
                    var length = arr.Length;
                    writer.Write(length);
                    for (int i = 0; i < length; i++)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(arr.GetValue(i)));
                    }
                },
                (type, reader) =>
                {
                    int length = reader.ReadInt32();
                    Type elementType = type.GetElementType();
                    Array array = Array.CreateInstance(elementType, length);

                    for (int i = 0; i < length; i++)
                    {
                        var obj = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), elementType);
                        array.SetValue(obj, i);
                    }
                    return array;
                },
                typeof(Array));

                //List
                RegisterResolve((obj, writer) =>
                {
                    var list = obj as IList;
                    //step 1 - write length
                    writer.Write(list.Count);
                    int i = 0;
                    //step 2 - write items
                    foreach (var item in list)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(item));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();
                    var list = type.CreateInstance();
                    var itemType = list.GetType().GetGenericArguments()[0];
                    //step 2 - read items
                    for (int i = 0; i < length; i++)
                    {
                        list.InvokeMethod("Add", QC.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
                    }
                    return list;
                },
                typeof(List<>));

                //Stack
                RegisterResolve((obj, writer) =>
                {
                    var pi = obj.GetType().GetProperty("Count");
                    var length = (int)pi.GetValue(obj);
                    var arr = Array.CreateInstance(obj.GetType().GetGenericArguments()[0], length);
                    obj.InvokeMethod("CopyTo", arr, 0);
                    Array.Reverse(arr);
                    //step 1 - write length
                    writer.Write(length);
                    //step 2 - write values
                    for (int i = 0; i < length; i++)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(arr.GetValue(i)));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();
                    var itemType = type.GetGenericArguments()[0];
                    var obj = type.CreateInstance();
                    //step 2 - read values
                    for (int i = 0; i < length; i++)
                    {
                        obj.InvokeMethod("Push", QC.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
                    }

                    return obj;
                },
                typeof(Stack<>));

                //Queue
                RegisterResolve((obj, writer) =>
                {
                    var list = obj as ICollection;
                    //step 1 - write length
                    writer.Write(list.Count);
                    int i = 0;
                    //step 2 - write items
                    foreach (var item in list)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(item));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();
                    var list = type.CreateInstance();
                    var itemType = list.GetType().GetGenericArguments()[0];
                    //step 2 - read items
                    for (int i = 0; i < length; i++)
                    {
                        list.InvokeMethod("Enqueue", QC.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
                    }
                    return list;
                },
                typeof(Queue<>));

                //LinkedList
                RegisterResolve((obj, writer) =>
                {
                    var list = obj as ICollection;
                    //step 1 - write length
                    writer.Write(list.Count);
                    int i = 0;
                    //step 2 - write items
                    foreach (var item in list)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(item));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();
                    var list = type.CreateInstance();
                    var itemType = list.GetType().GetGenericArguments()[0];
                    var methods = list.GetType().GetMethods();
                    MethodInfo mi = null;
                    foreach (var method in methods)
                        if (method.Name == "AddLast" && method.GetParameters().Length == 1)
                        {
                            mi = method;
                            break;
                        }
                    //step 2 - read items
                    for (int i = 0; i < length; i++)
                    {
                        mi.Invoke(list, new[] { QC.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType) });
                    }
                    return list;
                },
                typeof(LinkedList<>));

                #endregion

                #region IDictionary<,>
                //Dictionary
                RegisterResolve((obj, writer) =>
                {
                    var dictionary = obj as IDictionary;
                    //step 1 - write length
                    writer.Write(dictionary.Count);
                    //step 2 - write keysValuePairs
                    foreach (var key in dictionary.Keys)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(key));
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(dictionary[key]));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();

                    var dictionary = type.CreateInstance();
                    var keyType = dictionary.GetType().GetGenericArguments()[0];
                    var valueType = dictionary.GetType().GetGenericArguments()[1];

                    //step 2 - read keyValue pair
                    for (int i = 0; i < length; i++)
                    {
                        var key = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                        var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                        dictionary.InvokeMethod("Add", key, value);
                    }

                    return dictionary;
                },
                typeof(Dictionary<,>));

                //ConcurrentDictionary
                RegisterResolve((obj, writer) =>
                {
                    var dictionary = obj as IDictionary;
                    //step 1 - write length
                    writer.Write(dictionary.Count);
                    //step 2 - write keysValuePairs
                    foreach (var key in dictionary.Keys)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(key));
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(dictionary[key]));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();

                    var dictionary = type.CreateInstance();
                    var keyType = dictionary.GetType().GetGenericArguments()[0];
                    var valueType = dictionary.GetType().GetGenericArguments()[1];

                    //step 2 - read keyValue pair
                    for (int i = 0; i < length; i++)
                    {
                        var key = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                        var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                        dictionary.InvokeMethod("TryAdd", key, value);
                    }

                    return dictionary;
                },
                typeof(ConcurrentDictionary<,>));

                //ImmutableDictionary
                RegisterResolve((obj, writer) =>
                {
                    var dictionary = obj as IDictionary;
                    //step 1 - write length
                    writer.Write(dictionary.Count);
                    //step 2 - write keysValuePairs
                    foreach (var key in dictionary.Keys)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(key));
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(dictionary[key]));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();
                    
                    var keyType = type.GetGenericArguments()[0];
                    var valueType = type.GetGenericArguments()[1];
                    var dictionary = typeof(ImmutableDictionary).InvokeStaticGenericMethod("Create", new[] { keyType, valueType }, 0);

                    //step 2 - read keyValue pair
                    for (int i = 0; i < length; i++)
                    {
                        var key = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                        var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                        dictionary = dictionary.InvokeMethod("Add", key, value);
                    }

                    return dictionary;
                },
                typeof(ImmutableDictionary<,>));
                #endregion

                //Hashtable
                RegisterResolve((obj, writer) =>
                {
                    var hashtable = obj as Hashtable;
                    //step 1 - write length
                    writer.Write(hashtable.Count);
                    //step 2 - write keysValuePairs
                    foreach (var key in hashtable.Keys)
                    {
                        var value = hashtable[key];
                        // 1 - write key type
                        writer.Write(key.GetType().FullName);
                        // 2 - write key
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(key));
                        // 3 - check if value is null
                        if (value == null)
                            writer.Write(false); // Has value | false == null
                        else
                        {
                            writer.Write(true); //  Has value | true != null
                            writer.Write(value.GetType().FullName);
                            writer.WriteBinaryVarNative(QC.ConvertToBinary(value));
                        }
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();

                    var hashtable = new Hashtable();

                    //step 2 - read keyValue pair
                    for (int i = 0; i < length; i++)
                    {
                        // 1 - read key type
                        var keyType = reader.ReadString().FindTypeFromAllAssemblies();
                        // 2 - read key
                        var key = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                        if (reader.ReadBoolean())// Has value | true != null
                        {
                            var valueType = reader.ReadString().FindTypeFromAllAssemblies();
                            var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                            hashtable.Add(key, value);
                        }
                        else                     // Has value | false == null
                        {
                            hashtable.Add(key, null);
                        }
                    }

                    return hashtable;
                },
                typeof(Hashtable));

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
                    writer.WriteResolve(underlyingValue);
                },
                (type, reader) =>
                {
                    var underlyingType = Enum.GetUnderlyingType(type);
                    var obj = reader.ReadResolve(underlyingType);
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
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(arr.GetValue(i)));
                    }
                },
                (type, reader) =>
                {
                    var length = reader.ReadInt32();
                    var itemType = type.GetGenericArguments()[0];
                    var arr = Array.CreateInstance(itemType, length);
                    for (int i = 0; i < length; i++)
                    {
                        arr.SetValue(QC.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType), i);
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
                    writer.WriteResolve(plane.Normal);
                    writer.Write(plane.D);
                },
                (type, reader) =>
                {
                    return new Plane((Vector3)reader.ReadResolve(typeof(Vector3)), reader.ReadSingle());
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
                    writer.WriteResolve(q.BaseUtcOffset);
                    writer.Write(q.DisplayName);
                    writer.Write(q.StandardName);
                },
                (type, reader) =>
                {
                    return TimeZoneInfo.CreateCustomTimeZone(reader.ReadString(), (TimeSpan)reader.ReadResolve(typeof(TimeSpan)), reader.ReadString(), reader.ReadString());
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
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(value));
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
                        var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), types[i]);
                        SetValueTuple(tuple, value, i);
                    }
                    return tuple;
                },
                typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>), typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>));

                //Tuple
                RegisterResolve((obj, writer) =>
                {
                    var vt = CallToValueTuple(obj);
                    writer.WriteResolve(vt);
                },
                (type, reader) =>
                {
                    var vttype = TupleToValueTupleType(type);
                    var vt = reader.ReadResolve(vttype);
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
