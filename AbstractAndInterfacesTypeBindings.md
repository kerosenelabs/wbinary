
---
### *Binding types that are interfaces, abstract classes, or classes that other types inherit from*.
> *������� �����, ���������� ������������, ������������ ��������, ��� ��������, �� ������� ����������� ������ ����.*

###### Processing interfaces requires more memory and performance costs, so it's best to use custom type bindings.
> ��� ��������� ����������� ��������� ������ ������ �� ������ � ������������������, �������������� ����� ����� ������������ ���������������� �������� �����.
```csharp
    public class SourceEnumerableClass
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


    var rx = new SourceEnumerableClass();
    var bf = QC.Serialize(rx);

    var txList = QC.Deserialize<ListEnumerableClass>(bf);
    var txStack = QC.Deserialize<StackEnumerableClass>(bf);
    var txArray = QC.Deserialize<ArrEnumerableClass>(bf);
```
###### It is worth noting that interface types, abstract and static classes, as well as the Hashtable type cannot be passed to the constructor of the TypeBinding attribute.
> ����� ��������, ��� � ����������� �������� TypeBinding �� ����� ������������ ���� �����������, ����������� � ����������� �������, � ����� ��� Hashtable.

---