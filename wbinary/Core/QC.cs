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
using QuickC.Types;

namespace QuickC.Core
{
    public static class QC
    {
        public static TypeResolver TypeResolver { get; set; } = new TypeResolver();
        private static QCLocal _local = new QCLocal(TypeResolver);
        public static string VersionPrefix => $"{Vnfo.Major}.{Vnfo.Minor}";
        public static byte[] Serialize<T>(T obj, bool useCompression = true)
        {
            return _local.Serialize(obj, useCompression);
        }
        public static async Task<byte[]> SerializeAsync<T>(T obj, bool useCompression = true)
        {
            return await _local.SerializeAsync(obj, useCompression);
        }
        public static T? Deserialize<T>(byte[] source, bool ignoreVersion = false)
        {
            return _local.Deserialize<T>(source, ignoreVersion);
        }
        public static async Task<T?> DeserializeAsync<T>(byte[] source)
        {
            return await _local.DeserializeAsync<T>(source);
        }
        public static BinaryVar ConvertToBinary(object? value)
        {
            return _local.ConvertToBinary(value);
        }
        public static T? ConvertFromBinary<T>(BinaryVar buffer)
        {
            return _local.ConvertFromBinary<T>(buffer);
        }
        public static object? ConvertFromBinary(BinaryVar buffer, Type type)
        {
            return _local.ConvertFromBinary(buffer, type);
        }
    }
}
