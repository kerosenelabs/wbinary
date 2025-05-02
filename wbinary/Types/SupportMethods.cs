using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Types
{
    partial class TypeResolver
    {
        //tuples
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
        private static Type TupleToValueTupleType(Type tupleType)
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
        private static object CallToValueTuple(object tuple)
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
        private static object CallToTuple(object valueTuple)
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
        //end tuples
    }
}
