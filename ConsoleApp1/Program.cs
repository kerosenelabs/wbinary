using System.Numerics;
using System.Runtime.CompilerServices;
using wbinary.Abstract;
using wbinary.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rx = new SomeClass(0, 1, "jtk12");

            var bf = QC.Serialize(rx);
            Console.WriteLine(bf.Length);
            var tx = QC.Deserialize<SomeClass>(bf);
            Console.WriteLine(tx.ToString());
        }
    }


    public class SomeClass : IBufferingShort
    {
        private int i;
        private int j;
        private string jtk;
        public SomeClass() 
        {
        }

        public SomeClass(int I, int J, string JTK)
        {
            i = I; j = J; jtk = JTK;
        }
        public void ReadFromBuffer(BufferNumerableShort buffer)
        {
            i = buffer.ReadNext<int>();
            j = buffer.ReadNext<int>();
            jtk = buffer.ReadNext<string>();
        }

        public void WriteToBuffer(BufferNumerableShort buffer)
        {
            buffer.WriteNext(i);
            buffer.WriteNext(j);
            buffer.WriteNext(jtk);
        }
    }
}
