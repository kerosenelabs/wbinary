using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Extensions
{
    internal static class Extensions
    {
        public static Type? FindTypeFromAllAssemblies(this string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == fullTypeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public static object CreateInstance(this Type objectType, params Type[] genericArguments)
        {
            if(genericArguments.Length < 1)
                return Activator.CreateInstance(objectType);
            else
                return Activator.CreateInstance(objectType.MakeGenericType(genericArguments));
        }

        public static object CreateInstanceWithArgs(this Type objectType, params object?[] args)
        {
            return Activator.CreateInstance(objectType, args);
        }
        public static object CreateInstanceWithArgs(this Type objectType, Type[] genericArguments, params object?[] args)
        {
            return Activator.CreateInstance(objectType.MakeGenericType(genericArguments), args);
        }
        public static object CreateInstanceWithArgs(this Type objectType, Type genericArgument, params object?[] args)
        {
            return Activator.CreateInstance(objectType.MakeGenericType(genericArgument), args);
        }

        public static object? InvokeMethod(this Type type, object obj, string methodName, params object[] parameters)
        {
            return type.GetMethod(methodName).Invoke(obj, parameters);
        }
        public static object? InvokeGenericMethod(this Type type, object obj, string methodName, Type[] genericTypes, params object[] parameters)
        {
            return type.GetMethod(methodName).MakeGenericMethod(genericTypes).Invoke(obj, parameters);
        }
        public static object? InvokeGenericMethod(this Type type, object obj, string methodName, Type genericType, params object[] parameters)
        {
            return type.GetMethod(methodName).MakeGenericMethod(genericType).Invoke(obj, parameters);
        }
        public static object? InvokeMethod(this object obj, string methodName, params object[] parameters)
        {
            var type = obj.GetType();
            return type.GetMethod(methodName).Invoke(obj, parameters);
        }
        public static object? InvokeGenericMethod(this object obj, string methodName, Type[] genericTypes, params object[] parameters)
        {
            var type = obj.GetType();
            return type.GetMethod(methodName).MakeGenericMethod(genericTypes).Invoke(obj, parameters);
        }
        public static object? InvokeGenericMethod(this object obj, string methodName, Type genericType, params object[] parameters)
        {
            var type = obj.GetType();
            return type.GetMethod(methodName).MakeGenericMethod(genericType).Invoke(obj, parameters);
        }

        public static Type GetGenericType(this object instance)
        {
            return instance.GetType().GetGenericType();
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(this byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(mso, CompressionLevel.SmallestSize))
                {
                    CopyTo(msi, gs);
                }

                var arr = mso.ToArray();
                return arr;
            }
        }
        public static byte[] Unzip(this byte[] array)
        {
            using (var msi = new MemoryStream(array))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }
    }
}
