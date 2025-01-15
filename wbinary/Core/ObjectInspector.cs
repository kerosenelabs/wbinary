using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace wbinary.Core
{
    public class ObjectInspector
    {
        public object Instance { get; set; }
        public Type InstanceType { get; set; }

        public ObjectInspector(object inst)
        {
            Instance = inst;
            InstanceType = inst.GetType();
        }

        public PtrNodeValue[] Inspect()
        {
            var fields = InstanceType.GetFields();
            var properties = InstanceType.GetProperties();
            var pointers = new List<int>();
            var nodes = new List<PtrNodeValue>();
            int ptr = 0;
            foreach (var item in properties)
            {
                if (!item.CanWrite || !item.CanRead)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NoPointerAttribute>();
                if (attrNoPtr != null)
                    continue;
                int curPtr = ptr;
                var attrPtr = item.GetCustomAttribute<PointerAttribute>();
                if (attrPtr != null)
                {
                    curPtr = attrPtr.Ptr;
                    if (pointers.Contains(curPtr))
                        throw new ArgumentException($"The pointer '{curPtr}' specified in the 'PointerAttribute' attribute in a '{InstanceType.FullName}' object has been used for marking up several times.");
                }
                pointers.Add(curPtr);
                nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance)));
                ptr = ExcludePtr(++ptr, pointers);
            }
            foreach (var item in fields)
            {
                if (!item.IsPublic || item.IsInitOnly)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NoPointerAttribute>();
                if (attrNoPtr != null)
                    continue;
                int curPtr = ptr;
                var attrPtr = item.GetCustomAttribute<PointerAttribute>();
                if (attrPtr != null)
                {
                    curPtr = attrPtr.Ptr;
                    if (pointers.Contains(curPtr))
                        throw new ArgumentException($"The pointer '{curPtr}' specified in the 'PointerAttribute' attribute in a '{InstanceType.FullName}' object has been used for marking up several times.");
                }
                pointers.Add(curPtr);
                nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance)));
                ptr = ExcludePtr(++ptr, pointers);
            }
            return nodes.ToArray();
        }
        private static int ExcludePtr(int ptr, IEnumerable<int> exclPtr)
        {
            if(exclPtr.Contains(ptr))
            {
                ptr = ptr + 1;
                return ExcludePtr(ptr, exclPtr);
            }
            else
                return ptr;
        }
        public static PtrNode[] Inspect(Type objType)
        {
            var nodes = new List<PtrNode>();

            var fields = objType.GetFields();
            var properties = objType.GetProperties();
            var pointers = new List<int>();
            int ptr = 0;
            foreach (var item in properties)
            {
                if (!item.CanWrite || !item.CanRead)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NoPointerAttribute>();
                if (attrNoPtr != null)
                    continue;
                int curPtr = ptr;
                var attrPtr = item.GetCustomAttribute<PointerAttribute>();
                if (attrPtr != null)
                {
                    curPtr = attrPtr.Ptr;
                    if (pointers.Contains(curPtr))
                        throw new ArgumentException($"The pointer '{curPtr}' specified in the 'PointerAttribute' attribute in a '{objType.FullName}' object has been used for marking up several times.");
                    pointers.Add(curPtr);
                }
                nodes.Add(new PtrNode(item, curPtr));
                ptr++;
            }
            foreach (var item in fields)
            {
                if (!item.IsPublic || item.IsInitOnly)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NoPointerAttribute>();
                if (attrNoPtr != null)
                    continue;
                int curPtr = ptr;
                var attrPtr = item.GetCustomAttribute<PointerAttribute>();
                if (attrPtr != null)
                {
                    curPtr = attrPtr.Ptr;
                    if (pointers.Contains(curPtr))
                        throw new ArgumentException($"The pointer '{curPtr}' specified in the 'PointerAttribute' attribute in a '{objType.FullName}' object has been used for marking up several times.");
                    pointers.Add(curPtr);
                }
                nodes.Add(new PtrNode(item, curPtr));
                ptr++;
            }

            return nodes.ToArray();
        }
    }

    public class PtrNode
    {
        public int Ptr { get; internal set; }
        public Type ValueType { get; internal set; }
        internal MemberInfo Source { get; set; }
        public PtrNode(MemberInfo source, int ptr)
        {
            Ptr = ptr;
            Source = source;
            if (source is PropertyInfo)
            {
                ValueType = ((PropertyInfo)source).PropertyType;
            }
            else if (source is FieldInfo)
            {
                ValueType = ((FieldInfo)source).FieldType;
            }
            else
                throw new ArgumentException("Only a property or a field can be accepted as a MemberInfo.");
        }
        public object? GetValue(object instance)
        {
            if (Source is PropertyInfo)
            {
                return ((PropertyInfo)Source).GetValue(instance);
            }
            else if (Source is FieldInfo)
            {
                return ((FieldInfo)Source).GetValue(instance);
            }
            else
                throw new ArgumentException("Only a property or a field can be accepted as a MemberInfo.");
        }
        public void SetValue(object instance, object? value)
        {
            if (Source is PropertyInfo)
            {
                ((PropertyInfo)Source).SetValue(instance, value);
            }
            else if (Source is FieldInfo)
            {
                ((FieldInfo)Source).SetValue(instance, value);
            }
            else
                throw new ArgumentException("Only a property or a field can be accepted as a MemberInfo.");
        }
    }
    public class PtrNodeValue : PtrNode
    {
        public object? Value { get; }
        public PtrNodeValue(MemberInfo source, int ptr, object? value) : base(source, ptr)
        {
            Value = value;
        }
    }
}
