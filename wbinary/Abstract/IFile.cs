using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Abstract
{
    public interface IFile
    {
        void WriteAllBytes(string path, byte[] content);
        void WriteAllText(string path, string content);
    }
}
