using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickC.Core;

namespace QuickC.Abstract
{
    public interface IBuffering
    {
        void OnWriteToBuffer(ObjectBuffer buffer);
        void OnReadFromBuffer(ObjectBuffer buffer);
    }
}
