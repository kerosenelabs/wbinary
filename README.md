
---

### *Fast usage*
> *Быстрое использование*

###### For quick use, the static QC (QuickC) class from the webinary.Core namespace is used. It also allows you to significantly compress the byte array using Deflate.
> Для быстрого использования используется статический класс QC (QuickC) из пространства имен webinary.Core. Это также позволяет значительно сжать массив байт посредством Deflate.
```csharp
byte[] binary = QC.Serialize(<object>);
var obj = QC.Deserialize<T>(binary);
```

---

### *Supported types*
>*Поддерживаемые типы*

| Type                              | Support | Version |
| --------------------------------- | ------- | ------- |
| ==All primitives with nullables== | ✅       | 0.12    |
| IBuffering                        | ✅       | 0.12    |
| ==Array with nullables==          | ✅       | 0.12    |
| Dictionary                        | ✅       | 0.12    |
| List                              | ✅       | 0.12    |
| DateTime                          | ✅       | 0.12    |
###### If the required data types are missing in the latest version, it is possible to add them yourself.
> Если в последней версии отсутствуют требуемые типы данных, есть возможность добавить их самостоятельно.
```csharp
QC.TypeResolver.RegisterResolve(
    (obj, writer) =>
    {
		//some code...
    },
    (type, reader) =>
    {
	    //some code...
        return <object>;
    },
    params Type[] <allowedTypes>);
```
###### Example of adding resolve data types based on IBuffering.
> Пример добавления разрешения типов данных на основе IBuffering.
```csharp
QC.TypeResolver.RegisterResolve((obj, writer) =>
{
    var inspector = new ObjectInspector(obj);
    var nodes = inspector.Inspect();
    foreach (var node in nodes)
    {
        QC.WriteVarBuffer(QC.ConvertToBinary(node.Value, node.Ptr), writer);
    }

    var buffering = obj as IBuffering;
    var bf = new BufferNumerable();
    buffering.WriteToBuffer(bf);
    //count
    writer.Write(bf.Buffer.Count);
    foreach (var varBf in bf.Buffer)
    {
        writer.Write(varBf.Value.Length);
        writer.Write(varBf.Key);
        writer.Write(varBf.Value);
    }
},
(type, reader) =>
{
    var nodes = ObjectInspector.Inspect(type);
    var obj = type.CreateInstance();
    foreach (var node in nodes)
    {
        var val = QC.ConvertFromBinary(QC.ReadVarBuffer(reader), node.ValueType);
        node.SetValue(obj, val);
    }
    var bf = new BufferNumerable();
    var buffering = obj as IBuffering;
    //count
    var count = reader.ReadInt32();
    for (var i = 0; i < count; i++)
    {
        var length = reader.ReadInt32();
        var key = reader.ReadInt32();
        byte[] varBf = reader.ReadBytes(length);
        bf[key] = VarBuffer.FromBinary(varBf);
    }
    buffering.ReadFromBuffer(bf);

    return obj;
},
typeof(IBuffering));
```
###### Example of adding resolve data types based on Dictionary.
> Пример добавления разрешения типов данных на основе Dictionary.
```csharp
RegisterResolve((obj, writer) =>
{
    var dictionary = obj as IDictionary;
    writer.Write(dictionary.Count);

    int ptr = 0;
    foreach (var key in dictionary.Keys)
    {
        QC.WriteVarBuffer(QC.ConvertToBinary(key, ptr++), writer);
    }
    ptr = 0;
    foreach (var val in dictionary.Values)
    {
        QC.WriteVarBuffer(QC.ConvertToBinary(val, ptr++), writer);
    }
},
(type, reader) =>
{
    var length = reader.ReadInt32();

    var dictionary = type.CreateInstance();
    var keyType = dictionary.GetType().GetGenericArguments()[0];
    var valueType = dictionary.GetType().GetGenericArguments()[1];
    var keys = Array.CreateInstance(keyType, length);
    var values = Array.CreateInstance(valueType, length);

    for (int i = 0; i < length; i++)
    {
        keys.SetValue(QC.ConvertFromBinary(QC.ReadVarBuffer(reader), keyType), i);
    }

    for (int i = 0; i < length; i++)
    {
        values.SetValue(QC.ConvertFromBinary(QC.ReadVarBuffer(reader), valueType), i);
    }

    for (int i = 0; i < length; i++)
    {
        dictionary.InvokeMethod("Add", keys.GetValue(i), values.GetValue(i));
    }

    return dictionary;
},
typeof(IDictionary));
```
###### This implementation uses the VarBuffer structure, which contains data about the pointer to the field. In this case, the classes themselves are the schema.
> В данной реализации используется структура VarBuffer, которая содержит данные об указателе на поле, в данном случае схемой являются сами классы.
```csharp
//first app
VarBuffer varBf = QC.ConvertToBinary(SomeObject, <pointer:int>);
byte[] raw = varBf.ToBinary();
//some code...
Send(raw);

//second app
//some code...
byte[] raw = Receive();
VarBuffer varBf = VarBuffer.FromBinary(raw);
SomeObject = QC.ConvertFromBinary<SomeObject>(varBf);
```
###### A little more about QuickC. This one also allows you to write Var Buffer instead of the usual BinaryWriter values.Write() or BinaryReader.ReadInt32().
> Еще немного о QuickC. Данный также позволяет записывать Var Buffer, вместо обычных значений BinaryWriter.Write() или, например, BinaryReader.ReadInt32().
```csharp
//write VarBuffer in QC.TypeResolver.RegisterResolve()
(obj, writer) =>
{
	SomeObject sObj = obj as SomeObject;
	sObj.SomeValue = 0;
	int rootPointer = 0;
	//some code...
	sObj.SomeValue = 15;
	sObj.NoPointerValue = 16;
	
	VarBuffer varBf = QC.ConvertToBinary(sObj, rootPointer);
	VarBuffer varBfNoPointer = QC.ConvertToBinary(sObj.NoPointerValue, 0);
    QC.WriteVarBuffer(varBf, writer);
    QC.WriteVarBuffer(varBfNoPointer, writer);
},
//read VarBuffer in QC.TypeResolver.RegisterResolve()
(type, reader) =>
{
	var varBf = QC.ReadVarBuffer(reader);
	var varBfNoPointer = QC.ReadVarBuffer(reader);
    var obj = (SomeObject)QC.ConvertFromBinary(varBf, type);
    obj.NoPointerValue = QC.ConvertFromBinary<int>(varBfNoPointer);
    Console.WriteLine(obj.SomeValue); //output 15
    Console.WriteLine(obj.NoPointerValue); //output 16
    return obj;
},
//set allowed types
typeof(SomeObject));
```
###### So, using the internal mechanism for writing values via VarBuffer allows you to write values to the buffer recursively and simply for resolve unsupported types.
> Таким образом, использование внутреннего механизма записи значений через VarBuffer позволяет рекурсивно и достаточно просто записывать значения в буфер для разрешения неподдерживаемых типов.

---

### *IBuffering Interface*
> *Интерфейс IBuffering*
###### You probably would like to use the private members of your objects as you would like to do yourself, unfortunately QuickC does not support private members, but there is a simple solution - use the object buffer using the IBuffering interface. For example, you have a class like this:
> Вероятно вы бы хотели пользоваться закрытыми членами ваших объектов как хотели бы этого сами, к сожалению QuickC не поддерживает приватные члены, но есть простое решение - использовать буфер объекта с помощью интерфейса IBuffering. Например у вас есть такой класс:

```csharp
internal class TestClass
{
    public int A {  get; set; }
    public int? B { get; set; }
    private List<string> _variants;
    public string[] Variants => _variants.ToArray();
}
```
###### The _variants field is not available to us, and the Variations property does not have a set modifier. To do this, let's change our class as follows:
> Поле _variants нам не доступно, а у свойства Variants нет модификатора set. Для этого изменим наш класс следующим образом:
```csharp
internal class TestClass : IBuffering
{
    public int A {  get; set; }
    public int? B { get; set; }
    private List<string> _variants;
    public string[] Variants => _variants.ToArray();
    public TestClass() { }
    public TestClass(params string[] variants)
    {
        _variants = new List<string>(variants);
    }
    public void WriteToBuffer(BufferNumerable buffer)
    {
        buffer[0] = QC.ConvertToBinary(_variants, 0);
    }

    public void ReadFromBuffer(BufferNumerable buffer)
    {
        _variants = QC.ConvertFromBinary<List<string>>(buffer[0]);
    }
}
```
###### Now the buffer will be automatically taken into account when using the static QuickC class:
> Теперь буфер будет автоматически учитываться при использовании статического класса QuickC:
```csharp
//first app
var tObjRx = new TestClass("one", "two", "three")
{
    A = 1,
    B = null //test with nullable 'int?' solution
};
byte[] raw = QC.Serialize(tObjRx);
Send(raw);

//second app
byte[] raw = Receive();
var tObjTx = QC.Deserialize<TestClass>(raw);
foreach (var variant in tObjTx.Variants)
{
    Console.Write(variant + " "); //one two three
}
Console.WriteLine();
Console.WriteLine(tObjTx.A); // 1
Console.WriteLine(tObjTx.B != null ? tObjTx.B : "object B is null"); //object B is null
```

---

### Compression
> *Сжатие*
###### Since binary formalization involves writing binary values as an array of bytes, it was necessary to use Deflate compression, which significantly reduces the size of the serialized data, but if you do not need compression, you can use the following methods:
> Так как бинарная формализация подразумевает собой запись бинарных значений в виде массива байт, соответственно потребовалось использовать сжатие Deflate, которое значительно снижает размер сериализуемых данных, но если вам не требуется сжатие, вы можете воспользоваться следующими способами:
```csharp
//serialize without compression
byte[] raw = QC.ConvertToBinary(testClass, 0).ToBinary(); //raw size - 117 bytes
//deserialize without compression
TestClass obj = QC.ConvertFromBinary<TestClass>(VarBuffer.FromBinary(raw));

//serialize with compression
byte[] raw = QC.Serialize(obj); //raw size - 55
//deserialize with compression
TestClass obj = QC.Deserialize<TestClass>(raw);
```
###### The example above used the most recent implementation of TestClass, which was listed above. The size of the compressed array is 55 bytes, the size of the uncompressed array is 117 bytes, the difference is 62 bytes due to the fact that most values occupy the array with zeros, etc.
> В примере выше использовалась самая последняя реализация TestClass, которая приводилась выше. Размер сжатого массива 55 байт, размер несжатого массива 117 байт,  разница в 62 байта из-за того, что большинство значений занимают массив нулями и т.д.


---

### Performance
> *Быстродействие*
###### The library's performance with and without compression is shown below:
> Быстродействие работы библиотеки со сжатием и без представлены ниже:
```csharp
//with compression
var buff = QC.Serialize(rx);
var tx = QC.Deserialize<TestClass>(buff);
//without compression
var buff2 = QC.ConvertToBinary(rx, 0).ToBinary();
var tx2 = QC.ConvertFromBinary<TestClass>(VarBuffer.FromBinary(buff2));
```

| Ms   | Ticks | Type           |
| ---- | ----- | -------------- |
| ~20  | ~15   | Compression    |
| ~0.1 | 0     | No compression |
###### Compressor code:
> Код компрессора:
```csharp
		private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096]; //buffer 4KB

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        public static byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(mso, CompressionLevel.SmallestSize))
                {
                    CopyTo(msi, gs);
                }

                var arr = mso.ToArray();
                return arr;
            }
        }
        public static byte[] Unzip(byte[] array)
        {
            using (var msi = new MemoryStream(array))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }
```
