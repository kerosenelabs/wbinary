using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Core
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
            var nodes = new List<PtrNodeValue>();
            int ptr = 0;
            foreach (var item in properties)
            {
                if (!item.CanWrite || !item.CanRead)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NotSerializeAttribute>();
                if (attrNoPtr != null)
                    continue;
                var attrBinding = item.GetCustomAttribute<TypeBindingAttribute>();
                int curPtr = ptr;
                if (attrBinding == null)
                    nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance)));
                else
                    nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance), attrBinding.TypeBind));
                ++ptr;
            }
            foreach (var item in fields)
            {
                if (!item.IsPublic || item.IsInitOnly)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NotSerializeAttribute>();
                if (attrNoPtr != null)
                    continue;
                var attrBinding = item.GetCustomAttribute<TypeBindingAttribute>();
                int curPtr = ptr;
                if (attrBinding == null)
                    nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance)));
                else
                    nodes.Add(new PtrNodeValue(item, curPtr, item.GetValue(Instance), attrBinding.TypeBind));
                ++ptr;
            }
            return nodes.ToArray();
        }
        public static PtrNode[] Inspect(Type objType)
        {
            var nodes = new List<PtrNode>();

            var fields = objType.GetFields();
            var properties = objType.GetProperties();
            int ptr = 0;
            foreach (var item in properties)
            {
                if (!item.CanWrite || !item.CanRead)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NotSerializeAttribute>();
                if (attrNoPtr != null)
                    continue;
                var attrBinding = item.GetCustomAttribute<TypeBindingAttribute>();
                int curPtr = ptr;
                if (attrBinding == null)
                    nodes.Add(new PtrNode(item, curPtr));
                else
                    nodes.Add(new PtrNode(item, curPtr, attrBinding.TypeBind));
                ptr++;
            }
            foreach (var item in fields)
            {
                if (!item.IsPublic || item.IsInitOnly)
                    continue;
                var attrNoPtr = item.GetCustomAttribute<NotSerializeAttribute>();
                if (attrNoPtr != null)
                    continue;
                var attrBinding = item.GetCustomAttribute<TypeBindingAttribute>();
                int curPtr = ptr;
                if (attrBinding == null)
                    nodes.Add(new PtrNode(item, curPtr));
                else
                    nodes.Add(new PtrNode(item, curPtr, attrBinding.TypeBind));
                ptr++;
            }

            return nodes.ToArray();
        }
    }

    public class PtrNode
    {
        public int Ptr { get; internal set; }
        public Type ObjectType { get; internal set; }
        internal MemberInfo Source { get; set; }
        public PtrNode(MemberInfo source, int ptr)
        {
            Ptr = ptr;
            Source = source;
            if (source is PropertyInfo)
            {
                ObjectType = ((PropertyInfo)source).PropertyType;
            }
            else if (source is FieldInfo)
            {
                ObjectType = ((FieldInfo)source).FieldType;
            }
            else
                throw new ArgumentException("Only a property or a field can be accepted as a MemberInfo.");
        }
        public PtrNode(MemberInfo source, int ptr, Type type)
        {
            Ptr = ptr;
            Source = source;
            ObjectType = type;
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
        public PtrNodeValue(MemberInfo source, int ptr, object? value, Type type) : base(source, ptr, type)
        {
            Value = value;
        }
    }
}
