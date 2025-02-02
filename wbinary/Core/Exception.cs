using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Core
{
    public class UnsupportedVersionException : Exception
    {
        public string CurrentVersion { get; }
        public string RequiredVersion { get; }
        public UnsupportedVersionException(Headers headers) : base($"The required version of QuickC for deserialization of binary data is {headers.Major}.{headers.Minor}, and the current version of QuickC is {Vnfo.Major}.{Vnfo.Minor}.")
        {
            CurrentVersion = QC.VersionPrefix;
            RequiredVersion = $"{headers.Major}.{headers.Minor}";
        }
    }
}
