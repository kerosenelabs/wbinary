using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace wbinary
{
    /// <summary>
    /// Not considered when buffering
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NoPointerAttribute : Attribute
    {
    }
    /// <summary>
    /// Adding a special pointer
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PointerAttribute : Attribute
    {
        public int Ptr {  get; set; }
        public PointerAttribute(int ptr) { Ptr = ptr; }
    }
}
