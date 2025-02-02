
---
### Resolve unsupported or custom types
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
###### Example of adding resolve data types based on Dictionary.
> Пример добавления разрешения типов данных на основе Dictionary.
```csharp
QC.TypeResolver.RegisterResolve((obj, writer) =>
                {
                    var dictionary = obj as IDictionary;
                    //step 1 - write length
                    writer.Write(dictionary.Count);
                    //step 2 - write keysValuePairs
                    foreach (var key in dictionary.Keys)
                    {
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(key));
                        writer.WriteBinaryVarNative(QC.ConvertToBinary(dictionary[key]));
                    }
                },
                (type, reader) =>
                {
                    //step 1 - read length
                    var length = reader.ReadInt32();

                    var dictionary = type.CreateInstance();
                    var keyType = dictionary.GetType().GetGenericArguments()[0];
                    var valueType = dictionary.GetType().GetGenericArguments()[1];

                    //step 2 - read keyValue pair
                    for (int i = 0; i < length; i++)
                    {
                        var key = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), keyType);
                        var value = QC.ConvertFromBinary(reader.ReadBinaryVarNative(), valueType);
                        dictionary.InvokeMethod("Add", key, value);
                    }

                    return dictionary;
                },
                typeof(Dictionary<,>));
```
###### To write and read data, you can use both writer and reader, as well as TypeResolverExtensions (used if you need to write null or non-primitive types, arrays, etc.) in the QuickC.Extensions namespace.
> Для записи и чтения данных вы можете использовать как writer и reader, так и TypeResolverExtensions (используется, если нужно записать null или не примитивные типы, массивы и т.д.) в пространстве имен QuickC.Extensions.
```csharp
//non-null value
    writer.Write("some string");
    var someString = reader.ReadString();
//null value
    someString = null;
    writer.WriteBinaryVar(someString);
    var someString2 = reader.ReadBinaryVar<string>();
//user class value
    var someClass = new SomeClass
    {
        Age = 66,
        Name = "Bob"
    };
    writer.WriteBinaryVar(someClass);
    var someClass2 = reader.ReadBinaryVar<SomeClass>();
```

---