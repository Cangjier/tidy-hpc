using TidyHPC.LiteJson;

namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 运行时字段
/// <para>当JsonDocument写入数据库时，会将JsonDocument转换成FieldRuntime，然后再通过FieldRuntime写入byte[]进数据库</para>
/// <para>当从数据库读取数据时，会先将数据库数据解析成FieldRuntime，然后再转换成Json，或者Xml之类的</para>
/// </summary>
internal class FieldRuntime:IDisposable
{
    /// <summary>
    /// 是否启用调试模式，该模式下会输出更多的日志
    /// </summary>
    public static bool Debug { get; set; } = false;

    /// <summary>
    /// 字段定义
    /// </summary>
    public Field Define;

    /// <summary>
    /// 字段值
    /// </summary>
    public object Value;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="define"></param>
    /// <param name="value"></param>
    public FieldRuntime(Field define, object value)
    {
        Define = define;
        Value = value;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public FieldRuntime()
    {
        Define = new();
        Value = null!;
    }

    public int SerializeToBuffer(byte[] buffer,int offset)
    {
        var type = Define.Type;
        if (Define.IsArray)
        {
            if (type == FieldType.Char)
            {
                var charArray = (string)Value;
                buffer.Serialize(charArray, offset, Define.ArrayLength);
                return Define.ArrayLength * sizeof(byte);
            }
            else if (type == FieldType.Byte)
            {
                var byteArray = (byte[])Value;
                byteArray.CopyTo(buffer, offset);
                return Define.ArrayLength * sizeof(byte);
            }
            else if (type == FieldType.Boolean)
            {
                var booleanArray = (bool[])Value;
                foreach (var item in booleanArray)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(bool);
                }
                return Define.ArrayLength * sizeof(bool);
            }
            else if (type == FieldType.Int32)
            {
                var int32Array = (int[])Value;
                foreach (var item in int32Array)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(int);
                }
                return Define.ArrayLength * sizeof(int);
            }
            else if (type == FieldType.Float)
            {
                var floatArray = (float[])Value;
                foreach (var item in floatArray)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(float);
                }
                return Define.ArrayLength * sizeof(float);
            }
            else if (type == FieldType.Double)
            {
                var doubleArray = (double[])Value;
                foreach (var item in doubleArray)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(double);
                }
                return Define.ArrayLength * sizeof(double);
            }
            else if (type == FieldType.Int64)
            {
                var int64Array = (long[])Value;
                foreach (var item in int64Array)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(long);
                }
                return Define.ArrayLength * sizeof(long);
            }
            else if (type == FieldType.ReferneceString)
            {
                var referenceStringArray = (long[])Value;
                foreach (var item in referenceStringArray)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(long);
                }
                return Define.ArrayLength * sizeof(long);
            }
            else if (type == FieldType.Guid)
            {
                var guidArray = (Guid[])Value;
                foreach (var item in guidArray)
                {
                    item.ToByteArray().CopyTo(buffer, offset);
                    offset += 16;
                }
                return Define.ArrayLength * 16;
            }
            else if (type == FieldType.MD5)
            {
                var md5Array = (byte[][])Value;
                foreach (var item in md5Array)
                {
                    item.CopyTo(buffer, offset);
                    offset += 16;
                }
                return Define.ArrayLength * 16;
            }
            else if (type == FieldType.DateTime)
            {
                var dateTimeArray = (long[])Value;
                foreach (var item in dateTimeArray)
                {
                    BitConverter.GetBytes(item).CopyTo(buffer, offset);
                    offset += sizeof(long);
                }
                return Define.ArrayLength * sizeof(long);
            }
            else
            {
                throw new InvalidCastException();
            }
        }
        else
        {
            if(type == FieldType.Byte)
            {
                buffer[offset] = (byte)Value;
                return sizeof(byte);
            }
            else if (type == FieldType.Char)
            {
                buffer[offset]= (byte)(char)Value;
                return sizeof(byte);
            }
            else if (type == FieldType.Boolean)
            {
                BitConverter.GetBytes((bool)Value).CopyTo(buffer, offset);
                return sizeof(bool);
            }
            else if (type == FieldType.Int32)
            {
                BitConverter.GetBytes((int)Value).CopyTo(buffer, offset);
                return sizeof(int);
            }
            else if (type == FieldType.Float)
            {
                BitConverter.GetBytes((float)Value).CopyTo(buffer, offset);
                return sizeof(float);
            }
            else if (type == FieldType.Double)
            {
                BitConverter.GetBytes((double)Value).CopyTo(buffer, offset);
                return sizeof(double);
            }
            else if (type == FieldType.Int64)
            {
                BitConverter.GetBytes((long)Value).CopyTo(buffer, offset);
                return sizeof(long);
            }
            else if (type == FieldType.ReferneceString)
            {
                BitConverter.GetBytes((long)Value).CopyTo(buffer, offset);
                return sizeof(long);
            }
            else if (type == FieldType.Guid)
            {
                var guid = (Guid)Value;
                guid.ToByteArray().CopyTo(buffer, offset);
                return 16;
            }
            else if (type == FieldType.MD5)
            {
                var md5 = (byte[])Value;
                md5.CopyTo(buffer, offset);
                return 16;
            }
            else if (type == FieldType.DateTime)
            {
                BitConverter.GetBytes((long)Value).CopyTo(buffer, offset);
                return sizeof(long);
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }

    public int DeserializeFromBuffer(byte[] buffer, int offset)
    {
        var type = Define.Type;
        if (Define.IsArray)
        {
            if (type == FieldType.Char)
            {
                Value = buffer.DeserializeString(offset, Define.ArrayLength);
                return Define.ArrayLength * sizeof(byte);
            }
            else if (type == FieldType.Byte)
            {
                var byteArray = new byte[Define.ArrayLength];
                Array.Copy(buffer, offset, byteArray, 0, Define.ArrayLength);
                Value = byteArray;
                return Define.ArrayLength * sizeof(byte);
            }
            else if (type == FieldType.Boolean)
            {
                var booleanArray = new bool[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    booleanArray[i] = BitConverter.ToBoolean(buffer, offset);
                    offset += sizeof(bool);
                }
                Value = booleanArray;
                return Define.ArrayLength * sizeof(bool);
            }
            else if (type == FieldType.Int32)
            {
                var int32Array = new int[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    int32Array[i] = BitConverter.ToInt32(buffer, offset);
                    offset += sizeof(int);
                }
                Value = int32Array;
                return Define.ArrayLength * sizeof(int);
            }
            else if (type == FieldType.Float)
            {
                var floatArray = new float[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    floatArray[i] = BitConverter.ToSingle(buffer, offset);
                    offset += sizeof(float);
                }
                Value = floatArray;
                return Define.ArrayLength * sizeof(float);
            }
            else if (type == FieldType.Double)
            {
                var doubleArray = new double[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    doubleArray[i] = BitConverter.ToDouble(buffer, offset);
                    offset += sizeof(double);
                }
                Value = doubleArray;
                return Define.ArrayLength * sizeof(double);
            }
            else if (type == FieldType.Int64)
            {
                var int64Array = new long[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    int64Array[i] = BitConverter.ToInt64(buffer, offset);
                    offset += sizeof(long);
                }
                Value = int64Array;
                return Define.ArrayLength * sizeof(long);
            }
            else if (type == FieldType.ReferneceString)
            {
                var referenceStringArray = new long[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    referenceStringArray[i] = BitConverter.ToInt64(buffer, offset);
                    offset += sizeof(long);
                }
                Value = referenceStringArray;
                return Define.ArrayLength * sizeof(long);
            }
            else if (type == FieldType.Guid)
            {
                var guidArray = new Guid[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    var bytes = new byte[16];
                    Array.Copy(buffer, offset, bytes, 0, 16);
                    guidArray[i] = new Guid(bytes);
                    offset += 16;
                }
                Value = guidArray;
                return Define.ArrayLength * 16;
            }
            else if (type == FieldType.MD5)
            {
                var md5Array = new byte[Define.ArrayLength][];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    var bytes = new byte[16];
                    Array.Copy(buffer, offset, bytes, 0, 16);
                    md5Array[i] = bytes;
                    offset += 16;
                }
                Value = md5Array;
                return Define.ArrayLength * 16;
            }
            else if (type == FieldType.DateTime)
            {
                var dateTimeArray = new long[Define.ArrayLength];
                for (int i = 0; i < Define.ArrayLength; i++)
                {
                    dateTimeArray[i] = BitConverter.ToInt64(buffer, offset);
                    offset += sizeof(long);
                }
                Value = dateTimeArray;
                return Define.ArrayLength * sizeof(long);
            }
            else
            {
                throw new InvalidCastException();
            }
        }
        else
        {
            if (type == FieldType.Byte)
            {
                Value = buffer[offset];
                return sizeof(byte);
            }
            else if (type == FieldType.Char)
            {
                Value = (char)buffer[offset];
                return sizeof(byte);
            }
            else if (type == FieldType.Boolean)
            {
                Value = BitConverter.ToBoolean(buffer, offset);
                return sizeof(bool);
            }
            else if (type == FieldType.Int32)
            {
                Value = BitConverter.ToInt32(buffer, offset);
                return sizeof(int);
            }
            else if (type == FieldType.Float)
            {
                Value = BitConverter.ToSingle(buffer, offset);
                return sizeof(float);
            }
            else if (type == FieldType.Double)
            {
                Value = BitConverter.ToDouble(buffer, offset);
                return sizeof(double);
            }
            else if (type == FieldType.Int64)
            {
                Value = BitConverter.ToInt64(buffer, offset);
                return sizeof(long);
            }
            else if (type == FieldType.ReferneceString)
            {
                Value = BitConverter.ToInt64(buffer, offset);
                return sizeof(long);
            }
            else if (type == FieldType.Guid)
            {
                var bytes = new byte[16];
                Array.Copy(buffer, offset, bytes, 0, 16);
                Value = new Guid(bytes);
                return 16;
            }
            else if (type == FieldType.MD5)
            {
                var bytes = new byte[16];
                Array.Copy(buffer, offset, bytes, 0, 16);
                Value = bytes;
                return 16;
            }
            else if (type == FieldType.DateTime)
            {
                Value = BitConverter.ToInt64(buffer, offset);
                return sizeof(long);
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }

    public async Task DeserializeFromJson(Database db,Json element,bool isBorrowString)
    {
        var type = Define.Type;
        if (Define.IsArray)
        {
            if (type == FieldType.Char)
            {
                Value = element.AsString ?? throw new InvalidCastException();
            }
            else
            {
                if (element.Count > Define.ArrayLength)
                {
                    throw new IndexOutOfRangeException();
                }
                if (type == FieldType.Byte)
                {
                    var byteArray = new byte[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element.GetArrayEnumerable())
                    {
                        byteArray[i] = item.AsByte;
                        i++;
                    }
                    Value = byteArray;
                }
                else if (type == FieldType.Boolean)
                {
                    var booleanArray = new bool[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        booleanArray[i] = item.AsBoolean;
                        i++;
                    }
                    Value = booleanArray;
                }
                else if (type == FieldType.Int32)
                {
                    var int32Array = new int[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        int32Array[i] = item.AsInt32;
                        i++;
                    }
                    Value = int32Array;
                }
                else if (type == FieldType.Float)
                {
                    var floatArray = new float[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        floatArray[i] = item.AsFloat;
                        i++;
                    }
                    Value = floatArray;
                }
                else if (type == FieldType.Double)
                {
                    var doubleArray = new double[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        doubleArray[i] = item.AsDouble;
                        i++;
                    }
                    Value = doubleArray;
                }
                else if (type == FieldType.Int64)
                {
                    var int64Array = new long[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        int64Array[i] = item.AsInt64;
                        i++;
                    }
                    Value = int64Array;
                }
                else if (type == FieldType.ReferneceString)
                {
                    var referenceStringArray = new long[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        if (isBorrowString)
                        {
                            referenceStringArray[i] = await db.StringHashSet.Borrow(item.AsString ?? throw new InvalidCastException());
                        }
                        else
                        {
                            referenceStringArray[i] = await db.StringHashSet.New(item.AsString ?? throw new InvalidCastException());
                        }
                        i++;
                    }
                    Value = referenceStringArray;
                }
                else if (type == FieldType.Guid)
                {
                    var guidArray = new Guid[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        guidArray[i] = Guid.Parse(item.AsString ?? throw new InvalidCastException());
                        i++;
                    }
                    Value = guidArray;
                }
                else if (type == FieldType.MD5)
                {
                    var md5Array = new byte[Define.ArrayLength][];
                    var i = 0;
                    foreach (var item in element)
                    {
                        md5Array[i] = Util.HexToBytes(item.AsString ?? throw new InvalidCastException());
                        i++;
                    }
                    Value = md5Array;
                }
                else if (type == FieldType.DateTime)
                {
                    var dateTimeArray = new long[Define.ArrayLength];
                    var i = 0;
                    foreach (var item in element)
                    {
                        dateTimeArray[i] = DateTime.Parse(item.AsString).Ticks;
                        i++;
                    }
                    Value = dateTimeArray;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
        }
        else
        {
            if(type == FieldType.Byte)
            {
                Value = element.AsByte;
            }
            else if (type == FieldType.Char)
            {
                var stringValue = element.AsString ?? throw new InvalidCastException();
                if (stringValue.Length == 0)
                {
                    throw new InvalidCastException();
                }
                Value = stringValue[0];
            }
            else if (type == FieldType.Boolean)
            {
                Value = element.AsBoolean;
            }
            else if (type == FieldType.Int32)
            {
                Value = element.AsInt32;
            }
            else if (type == FieldType.Float)
            {
                Value = element.AsFloat;
            }
            else if (type == FieldType.Double)
            {
                Value = element.AsDouble;
            }
            else if (type == FieldType.Int64)
            {
                Value = element.AsInt64;
            }
            else if (type == FieldType.ReferneceString)
            {
                if (Debug)
                {
                    Json self = Json.NewObject();
                    Define.SerializeToJson(self.GetOrCreateObject("field"));
                    self["value"] = element.ToString();
                    db.Logger.WriteLine($"// field new {self.ToString(false)}");
                }
                if (isBorrowString)
                {
                    Value = await db.StringHashSet.Borrow(element.AsString ?? throw new InvalidCastException());
                }
                else
                {
                    Value = await db.StringHashSet.New(element.AsString ?? throw new InvalidCastException());
                }
                
            }
            else if (type == FieldType.Guid)
            {
                Value = Guid.Parse(element.AsString ?? throw new InvalidCastException());
            }
            else if (type == FieldType.MD5)
            {
                Value = Util.HexToBytes(element.AsString ?? throw new InvalidCastException());
            }
            else if (type == FieldType.DateTime)
            {
                Value = DateTime.Parse(element.AsString).Ticks;
            }
            else
            {
                throw new InvalidCastException();
            }
        }
    }

    public void SerializeToJson(Json self)
    {
        Define.SerializeToJson(self.GetOrCreateObject("Define"));
        self["Value"] = Value.ToString() ?? "";
    }

    /// <summary>
    /// 释放引用
    /// <para>目前主要是引用字符串</para>
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task ReleaseReference(Database db)
    {
        if (Define.IsArray)
        {
            if (Define.Type == FieldType.ReferneceString)
            {
                foreach (var item in (long[])Value)
                {
                    await db.StringHashSet.Release(item);
                }
            }
        }
        else
        {
            if (Define.Type == FieldType.ReferneceString)
            {
                if (Debug)
                {
                    Json self = Json.NewObject();
                    Define.SerializeToJson(self.GetOrCreateObject("field"));
                    self["hashCode"] = (long)Value;
                    db.Logger.WriteLine($"// field releaseReference {self.ToString(false)}");
                }
                await db.StringHashSet.Release((long)Value);
            }
        }
    }

    public async Task IncreaseReference(Database db)
    {
        if (Define.IsArray)
        {
            if (Define.Type == FieldType.ReferneceString)
            {
                foreach (var item in (long[])Value)
                {
                    await db.StringHashSet.Increase(item);
                }
            }
        }
        else
        {
            if (Define.Type == FieldType.ReferneceString)
            {
                if (Debug)
                {
                    Json self = Json.NewObject();
                    Define.SerializeToJson(self.GetOrCreateObject("field"));
                    self["hashCode"] = (long)Value;
                    db.Logger.WriteLine($"// field increaseReference {self.ToString(false)}");
                }
                await db.StringHashSet.Increase((long)Value);
            }
        }
    }

    public void Dispose()
    {
        Value = null!;
    }
}
