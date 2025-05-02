using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Abstract
{
    public interface IDirectory
    {
        bool Exist(string path);
        void CreateDirectory(string path);
        void Delete(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
        DateTimeOffset? GetLastWriteTime(string path);

        void Move(string sourcePath, string destinationPath);
        void Copy(string sourcePath, string destinationPath);
    }
}
