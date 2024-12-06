namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 字段
/// </summary>
public struct Field
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 字段类型
    /// </summary>
    public FieldType Type { get; set; }

    /// <summary>
    /// 数组长度，0或者1表示非数组，大于1表示数组
    /// </summary>
    public int ArrayLength { get; set; }

    /// <summary>
    /// 映射类型
    /// </summary>
    public FieldMapType MapType { get; set; }

    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsArray => ArrayLength > 1;

    /// <summary>
    /// 获取字段大小
    /// </summary>
    /// <returns></returns>
    public readonly int GetSize()
    {
        if (ArrayLength > 1)
        {
            return Type.GetSize() * ArrayLength;
        }
        else
        {
            return Type.GetSize();
        }
    }

    /// <summary>
    /// 序列化到Json
    /// </summary>
    /// <param name="self"></param>
    public void SerializeToJson(LiteJson.Json self)
    {
        self["Name"] = Name;
        self["Type"] = (byte)Type;
        self["ArrayLength"] = ArrayLength;
        self["MapType"] = (byte)MapType;
    }

    /// <summary>
    /// 从Json中反序列化
    /// </summary>
    /// <param name="self"></param>
    public void DeserializeFromJson(LiteJson.Json self)
    {
        Name = self.Read("Name", string.Empty);
        Type = (FieldType)self.Read("Type", (byte)FieldType.Byte);
        ArrayLength = self.Read("ArrayLength", 0);
        MapType = (FieldMapType)self.Read("MapType", (byte)FieldMapType.None);
    }
}
