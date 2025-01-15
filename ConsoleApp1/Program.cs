using System.Numerics;
using System.Runtime.CompilerServices;
using wbinary.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rx = Tuple.Create("suka", 123, 1.0f);

            var bf = QC.Serialize(rx);
            Console.WriteLine(bf.Length);
            var tx = QC.Deserialize<Tuple<string, int, float>>(bf);
            Console.WriteLine(tx.ToString());
        }

        public static (string Str, int Ptr) Cont()
        {
            return ("osx", 123);
        }
    }

    public enum SomeEnum : byte
    {
        One,
        Two,
        Three
    }

}
