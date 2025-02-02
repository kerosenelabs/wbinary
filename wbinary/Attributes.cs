using QuickC.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace QuickC
{
    /// <summary>
    /// Not considered when buffering
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NotSerializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TypeBindingAttribute : Attribute
    {
        public Type TypeBind { get; set; }
        public TypeBindingAttribute(Type type)
        {
            if (type.IsInterface || type.IsAbstract || type.IsStatic() || type.Equals(typeof(Hashtable)))
                throw new Exception("The \"TypeBindingAttribute\" attribute cannot be used with interfaces, abstract and static classes, Hashtable.");
            TypeBind = type;
        }
    }
}
