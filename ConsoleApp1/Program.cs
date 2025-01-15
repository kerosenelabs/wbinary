using System.Numerics;
using wbinary.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rx = new Stack<int>();

            rx.Push(1);
            rx.Push(2);
            rx.Push(3);
            rx.Push(0);
            rx.Push(1);
            rx.Pop();

            var bf = QC.Serialize(rx);
            var tx = QC.Deserialize<Stack<int>>(bf);
            Console.WriteLine(tx.ToString());
        }
    }

    public enum SomeEnum : byte
    {
        One,
        Two, 
        Three
    }
}
