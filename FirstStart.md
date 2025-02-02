
---
### *Fast usage*
> *Быстрое использование*

###### For quick use, the static QC (QuickConvert) class from the wbinary.Core namespace is used. It also allows you to significantly compress the byte array using Deflate compression.
> Для быстрого использования используется статический класс QC (QuickConvert) из пространства имен wbinary.Core. Это также позволяет значительно сжать массив байт посредством Deflate сжатие.
```csharp
byte[] binary = QC.Serialize(<object>);
var obj = QC.Deserialize<T>(binary);
```

---