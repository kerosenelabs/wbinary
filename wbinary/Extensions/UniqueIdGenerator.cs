using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Extensions
{
    public static class UniqueIdGenerator
    {
        public static long GenerateUniqueId()
        {
            Guid guid = Guid.NewGuid();
            byte[] guidBytes = guid.ToByteArray();
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(guidBytes);
                long uniqueId = BitConverter.ToInt64(hashBytes, 0);
                return uniqueId;
            }
        }
    }
}
