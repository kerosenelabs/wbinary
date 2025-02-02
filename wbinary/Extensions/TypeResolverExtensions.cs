using QuickC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickC.Core.QC;

namespace QuickC.Extensions
{
    public static class TypeResolverExtensions
    {
        internal static void WriteResolve(this BinaryWriter writer, object value)
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
                    var resolver = TypeResolver.FindWriterResolve(value.GetType());
                    if (resolver != null)
                        resolver?.Invoke(value, writer);
                    else
                    {
                        resolver = TypeResolver.DefaultWriterResolve;
                        resolver?.Invoke(value, writer);
                    }
                    break;
            }
        }
        internal static object ReadResolve(this BinaryReader reader, Type elementType)
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
            var resolver = TypeResolver.FindReaderResolve(elementType);
            if (resolver != null)
                return resolver?.Invoke(elementType, reader);
            else
            {
                resolver = TypeResolver.DefaultReaderResolve;
                return resolver?.Invoke(elementType, reader);
            }
        }
        internal static void WriteBinaryVarNative(this BinaryWriter writer, BinaryVar buffer)
        {
            var binary = buffer.ToBinary();
            writer.Write(binary.Length);
            writer.Write(binary);
        }
        internal static BinaryVar ReadBinaryVarNative(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var binary = reader.ReadBytes(length);
            return BinaryVar.FromBinary(binary);
        }

        public static void WriteBinaryVar(this BinaryWriter writer, object obj)
        {
            var binary = QC.ConvertToBinary(obj).ToBinary();
            writer.Write(binary.Length);
            writer.Write(binary);
        }
        public static T ReadBinaryVar<T>(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            var binary = reader.ReadBytes(length);
            return QC.ConvertFromBinary<T>(BinaryVar.FromBinary(binary));
        }
        public static object ReadBinaryVar(this BinaryReader reader, Type objType)
        {
            var length = reader.ReadInt32();
            var binary = reader.ReadBytes(length);
            return QC.ConvertFromBinary(BinaryVar.FromBinary(binary), objType);
        }
    }
}
