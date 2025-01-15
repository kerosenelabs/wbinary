using wbinary.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //File.WriteAllBytes(@"C:\Users\kyrosine\Documents\test.bin", QC.Serialize("Тестовое сообщение"));

            var bf = File.ReadAllBytes(@"C:\Users\kyrosine\Documents\test.bin");
            Console.WriteLine(bf.Length);
            Console.WriteLine(QC.Deserialize<string>(bf));
        }
    }
}
