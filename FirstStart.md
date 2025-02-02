
---
### *Fast usage*
> *������� �������������*

###### For quick use, the static QC (QuickConvert) class from the wbinary.Core namespace is used. It also allows you to significantly compress the byte array using Deflate compression.
> ��� �������� ������������� ������������ ����������� ����� QC (QuickConvert) �� ������������ ���� wbinary.Core. ��� ����� ��������� ����������� ����� ������ ���� ����������� Deflate ������.
```csharp
byte[] binary = QC.Serialize(<object>);
var obj = QC.Deserialize<T>(binary);
```

---