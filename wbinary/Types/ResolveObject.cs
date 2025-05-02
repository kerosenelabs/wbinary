using QuickC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Types
{
    public delegate void ResolveTypeWrite(object obj, BinaryWriter writer);
    public delegate object ResolveTypeRead(Type type, BinaryReader reader);
    internal class ResolveObject
    {
        public ResolveTypeWrite? resolveTypeWrite { get; set; }
        public ResolveTypeRead? resolveTypeRead { get; set; }
        public Type[] allowTypes { get; set; }
        public bool TypeContains(Type type)
        {
            foreach (var x in allowTypes)
            {

                if (x.Equals(type))
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
