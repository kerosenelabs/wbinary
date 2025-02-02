
---
### *IBuffering Interface*
> *Интерфейс IBuffering*
###### You probably would like to use the private members of your objects as you would like to do yourself, unfortunately QuickC does not support private members, but there is a simple solution - use the object buffer using the IBuffering interface. For example, you have a class like this:
> Вероятно вы бы хотели пользоваться закрытыми членами ваших объектов как хотели бы этого сами, к сожалению QuickC не поддерживает приватные члены, но есть простое решение - использовать буфер объекта с помощью интерфейса IBuffering. Например у вас есть такой класс:

```csharp
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
```
###### Now the buffer will be automatically taken into account when using the static QuickC class:
> Теперь буфер будет автоматически учитываться при использовании статического класса QuickC:
```csharp
            var person1 = new ClassWithBuffer(18, "Tom");
            var bf = QC.Serialize(person1);

            var person2 = QC.Deserialize<ClassWithBuffer>(bf);
            Console.WriteLine(person2.ToString()); // Output is: "Name: Tom, Age: 18"
```

---