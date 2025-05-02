using QuickC.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using QuickC.Core;
using QuickC.Abstract;

namespace QuickC.Types
{
    partial class TypeResolver
    {
        internal QCLocal _qcConverter;
        public TypeResolver()
        {
            RegisterDefault();
            RegisterIBuffering();
            RegisterICollectionBase();
            RegisterDictionaryBase();
            RegisterHashtable();
            RegisterEnum();
            RegisterDateTimeBase();
            RegisterBigInt();
            RegisterComplex();
            RegisterGuid();
            RegisterUri();
            RegisterVector();
            RegisterMatrix();
            RegisterPlane();
            RegisterQuaternion();
            RegisterTimeZoneInfo();
            RegisterTuple();
        }

        private void RegisterDefault()
        {
            defaultResolveObject = new ResolveObject()
            {
                allowTypes = Array.Empty<Type>(),
                resolveTypeWrite = (obj, writer) =>
                {
                    var inspector = new ObjectInspector(obj);
                    var nodes = inspector.Inspect();
                    foreach (var node in nodes)
                    {
                        writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(node.Value));
                    }
                },
                resolveTypeRead = (type, reader) =>
                {
                    var nodes = ObjectInspector.Inspect(type);
                    var obj = type.CreateInstance();
                    foreach (var node in nodes)
                    {
                        var val = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), node.ObjectType);
                        node.SetValue(obj, val);
                    }
                    return obj;
                }
            };
        }
        private void RegisterIBuffering()
        {
            RegisterResolve((obj, writer) =>
            {
                var inspector = new ObjectInspector(obj);
                var nodes = inspector.Inspect();
                foreach (var node in nodes)
                {
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(node.Value));
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
                    var val = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), node.ObjectType);
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
        }
        private void RegisterICollectionBase()
        {
            //Array
            RegisterResolve((obj, writer) =>
            {
                var arr = obj as Array;
                var length = arr.Length;
                writer.Write(length);
                for (int i = 0; i < length; i++)
                {
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(arr.GetValue(i)));
                }
            },
            (type, reader) =>
            {
                int length = reader.ReadInt32();
                Type elementType = type.GetElementType();
                Array array = Array.CreateInstance(elementType, length);

                for (int i = 0; i < length; i++)
                {
                    var obj = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), elementType);
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(item));
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
                    list.InvokeMethod("Add", _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(arr.GetValue(i)));
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
                    obj.InvokeMethod("Push", _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(item));
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
                    list.InvokeMethod("Enqueue", _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType));
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(item));
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
                    mi.Invoke(list, new[] { _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType) });
                }
                return list;
            },
            typeof(LinkedList<>));
        }
        private void RegisterDictionaryBase()
        {
            //Dictionary
            RegisterResolve((obj, writer) =>
            {
                var dictionary = obj as IDictionary;
                //step 1 - write length
                writer.Write(dictionary.Count);
                //step 2 - write keysValuePairs
                foreach (var key in dictionary.Keys)
                {
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(key));
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(dictionary[key]));
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
                    var key = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                    var value = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(key));
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(dictionary[key]));
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
                    var key = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                    var value = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(key));
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(dictionary[key]));
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
                    var key = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                    var value = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                    dictionary = dictionary.InvokeMethod("Add", key, value);
                }

                return dictionary;
            },
            typeof(ImmutableDictionary<,>));
        }
        private void RegisterHashtable()
        {
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
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(key));
                    // 3 - check if value is null
                    if (value == null)
                        writer.Write(false); // Has value | false == null
                    else
                    {
                        writer.Write(true); //  Has value | true != null
                        writer.Write(value.GetType().FullName);
                        writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(value));
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
                    var key = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                    if (reader.ReadBoolean())// Has value | true != null
                    {
                        var valueType = reader.ReadString().FindTypeFromAllAssemblies();
                        var value = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
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
        }
        private void RegisterTuple()
        {
            //ValueTuple
            RegisterResolve((obj, writer) =>
            {
                var intf = obj as ITuple;
                var length = intf.Length;
                writer.Write(length);
                for (int i = 0; i < length; i++)
                {
                    var value = intf[i];
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(value));
                }
            },
            (type, reader) =>
            {
                var length = reader.ReadInt32();
                var tuple = type.CreateInstance();

                var intf = (ITuple)tuple;
                if (intf.Length != length)
                    throw new EndOfStreamException($"The tuple you are trying to deserialize has a length of '{length}' arguments, the requested type has '{intf.Length}' arguments.");

                var types = new List<Type>();
                var currentType = type;

                while (currentType != null && currentType.IsGenericType)
                {
                    var currentTypes = currentType.GetGenericArguments();
                    types.AddRange(currentTypes.Take(7));

                    currentType = currentTypes.Length == 8 ? currentTypes[7] : null;
                }

                for (int i = 0; i < length; i++)
                {
                    var value = _qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), types[i]);
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
        private void RegisterTimeZoneInfo()
        {
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
        }
        private void RegisterQuaternion()
        {
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
        }
        private void RegisterPlane()
        {
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
        }
        private void RegisterMatrix()
        {
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
        }
        private void RegisterVector()
        {
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
                cMe.Invoke(obj, new[] { arr });
                writer.Write(arr.Length);
                for (int i = 0; i < arr.Length; i++)
                {
                    writer.WriteBinaryVarNative(_qcConverter.ConvertToBinary(arr.GetValue(i)));
                }
            },
            (type, reader) =>
            {
                var length = reader.ReadInt32();
                var itemType = type.GetGenericArguments()[0];
                var arr = Array.CreateInstance(itemType, length);
                for (int i = 0; i < length; i++)
                {
                    arr.SetValue(_qcConverter.ConvertFromBinary(reader.ReadBinaryVarNative(), itemType), i);
                }
                return type.CreateInstanceWithArgs(arr);
            },
            typeof(Vector<>));
        }
        private void RegisterUri()
        {
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
        }
        private void RegisterGuid()
        {
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
        }
        private void RegisterComplex()
        {
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
        }
        private void RegisterBigInt()
        {
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
        }
        private void RegisterEnum()
        {
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
        }
        private void RegisterDateTimeBase()
        {
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
        }
    }
}
