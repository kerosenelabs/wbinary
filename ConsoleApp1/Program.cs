using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using QuickC;
using QuickC.Abstract;
using QuickC.Core;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("test nullable | null");
            //TestNullable(null);

            //Console.WriteLine("\r\ntest nullable | int.MaxValue");
            //TestNullable(int.MaxValue);

            //Console.WriteLine("\r\ntest Hashtable");
            //TestHashtable();

            Console.WriteLine("\r\ntest Dictionary");
            TestDictionary();

            //Console.WriteLine("\r\ntest buffering object | Tom :: 18");
            //TestBufferingObject();

            //Console.WriteLine("\r\ntest type binding to interfaces and abstract classes | Array<string> to IEnumerable<string> as List<string>");
            //TestTypeBinding();

            Console.WriteLine("\r\ntest enumerables | one two three");
            TestEnumerables();
            TestVersionWrite();
            Console.ReadKey(true);
        }
        static void TestTypeBinding()
        {
            var rx = new StartEnumerableClass();
            var bf = QC.Serialize(rx);
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx = QC.Deserialize<ListEnumerableClass>(bf);
            var tx2 = QC.Deserialize<StackEnumerableClass>(bf);
            var tx3 = QC.Deserialize<ArrEnumerableClass>(bf);
            Console.WriteLine("ListEnumerableClass:");
            foreach (var x in tx.Numerable)
            {
                Console.Write(x + " ");
            }
            Console.WriteLine("\r\nStackEnumerableClass:");
            foreach (var x in tx2.Numerable)
            {
                Console.Write(x + " ");
            }
            Console.WriteLine("\r\nArrEnumerableClass:");
            foreach (var x in tx3.Numerable)
            {
                Console.Write(x + " ");
            }
            Console.WriteLine();
        }
        static void TestBufferingObject()
        {
            var rx = new ClassWithBuffer(18, "Tom");
            var bf = QC.Serialize(rx);
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx = QC.Deserialize<ClassWithBuffer>(bf);
            Console.WriteLine(tx.ToString());
        }
        static void TestNullable(int? value)
        {
            var ov = QC.ConvertToBinary(value).ToBinary();
            int? second = QC.ConvertFromBinary<int?>(BinaryVar.FromBinary(ov));
            Console.WriteLine(second);
        }
        static void TestHashtable()
        {
            var hashTable = new Hashtable();
            hashTable.Add(1, 1);
            hashTable.Add(2, "two");
            hashTable.Add("three", new ClassWithBuffer(22, "Oliver"));
            hashTable.Add(true, null);
            var bf = QC.ConvertToBinary(hashTable).ToBinary();
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx = QC.ConvertFromBinary<Hashtable>(BinaryVar.FromBinary(bf));
            Console.WriteLine($"1 | {hashTable[1]}\r\n" +
                $"2 | {hashTable[2]}\r\n" +
                $"\"three\" | {hashTable["three"].ToString()}\r\n" +
                $"true | {hashTable[true]}");
        }
        static void TestDictionary()
        {
            var dict = new Dictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            dict.Add(3, null);
            dict.Add(4, "four");
            var bf = QC.ConvertToBinary(dict).ToBinary();
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx = QC.ConvertFromBinary<Dictionary<int, string>>(BinaryVar.FromBinary(bf));
            Console.WriteLine("Standart dictionary:");
            foreach (var kv in tx)
            {
                Console.Write($"'{kv.Key}' : '{kv.Value}', ");
            }
            Console.WriteLine();

            var dict2 = new ConcurrentDictionary<int, string>(dict);
            bf = QC.ConvertToBinary(dict2).ToBinary();
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx2 = QC.ConvertFromBinary<ConcurrentDictionary<int, string>>(BinaryVar.FromBinary(bf));
            Console.WriteLine("ConcurrentDictionary dictionary:");
            foreach (var kv in tx2)
            {
                Console.Write($"'{kv.Key}' : '{kv.Value}', ");
            }
            Console.WriteLine();

            var dict3 = ImmutableDictionary.Create<int, string>().Add(1, "one").Add(2, "two").Add(3, null);
            bf = QC.ConvertToBinary(dict3).ToBinary();
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var tx3 = QC.ConvertFromBinary<ImmutableDictionary<int, string>>(BinaryVar.FromBinary(bf));
            Console.WriteLine("ImmutableDictionary dictionary:");
            foreach (var kv in tx3)
            {
                Console.Write($"'{kv.Key}' : '{kv.Value}', ");
            }
            Console.WriteLine();
        }
        static void TestEnumerables()
        {
            var rxLinkedList = new LinkedList<string>();
            rxLinkedList.AddLast("one");
            rxLinkedList.AddLast("two");
            rxLinkedList.AddLast("three");
            var bf = QC.ConvertToBinary(rxLinkedList).ToBinary();
            Console.WriteLine("Buffer length is " + bf.Length + " bytes");
            var txLinkedList = QC.ConvertFromBinary<LinkedList<string>>(BinaryVar.FromBinary(bf));
            Console.WriteLine("LinkedList<string>:");
            foreach(var x in txLinkedList)
            {
                Console.Write(x + " ");
            }
            var rxQueue = new Queue<string>();
            rxQueue.Enqueue("one");
            rxQueue.Enqueue("two");
            rxQueue.Enqueue("three");
            bf = QC.ConvertToBinary(rxQueue).ToBinary();
            Console.WriteLine("\r\nBuffer length is " + bf.Length + " bytes");
            var txQueue = QC.ConvertFromBinary<Queue<string>>(BinaryVar.FromBinary(bf));
            Console.WriteLine("Queue<string>:");
            foreach (var x in txQueue)
            {
                Console.Write(x + " ");
            }
            Console.WriteLine();
        }

        static void TestVersionWrite()
        {
            //var rx = new List<int> { 1, 2, 3 };
            //File.WriteAllBytes("txt.t", QC.Serialize(rx));

            //var tx = QC.Deserialize<List<int>>(File.ReadAllBytes("txt.t"));
        }
    }


    public class ClassWithBuffer : IBuffering
    {
        private int _age;
        private string _name;
        public ClassWithBuffer(){}
        public ClassWithBuffer(int age, string name)
        {
            _age = age;
            _name = name;
        }
        public void OnReadFromBuffer(ObjectBuffer buffer)
        {
            _age = buffer.ReadNext<int>();
            _name = buffer.ReadNext<string>();
        }
        public void OnWriteToBuffer(ObjectBuffer buffer)
        {
            buffer.WriteNext(_age);
            buffer.WriteNext(_name);
        }
        public override string ToString()
        {
            return $"Name: {_name}, Age: {_age}";
        }
    }

    public class StartEnumerableClass
    {
        public string[] Array = {"1", "2", "3"};
    }
    public class ListEnumerableClass
    {
        [TypeBinding(typeof(List<string>))]
        public IEnumerable<string> Numerable { get; set; }
    }
    public class StackEnumerableClass
    {
        [TypeBinding(typeof(Stack<string>))]
        public IEnumerable<string> Numerable { get; set; }
    }
    public class ArrEnumerableClass
    {
        [TypeBinding(typeof(string[]))]
        public IEnumerable<string> Numerable { get; set; }
    }
}
