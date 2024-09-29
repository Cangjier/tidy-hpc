namespace TidyHPC.LiteDB.HashSets;

/// <summary>
/// 字符串记录
/// </summary>
internal struct StringRecord
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public StringRecord(byte[] buffer, int offset)
    {
        Length = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        ReferenceCount = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        Value = buffer.Skip(offset).Take(256).ToArray();
        offset += 256;
        NextRecordAddress = BitConverter.ToInt64(buffer, offset);
    }

    /// <summary>
    /// 字符串长度
    /// </summary>
    public int Length;

    /// <summary>
    /// 引用次数
    /// </summary>
    public int ReferenceCount;

    /// <summary>
    /// 字符串的值
    /// </summary>
    public byte[] Value;

    /// <summary>
    /// 是否需要下一个记录进行拼接字符串
    /// </summary>
    public long NextRecordAddress;

    /// <summary>
    /// StringRecord的大小
    /// </summary>
    public const int Size = sizeof(int) + sizeof(int) + 256 + sizeof(long);

    /// <summary>
    /// 将数据写入到buffer中
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Length).CopyTo(buffer, offset);
        offset += sizeof(int);
        BitConverter.GetBytes(ReferenceCount).CopyTo(buffer, offset);
        offset += sizeof(int);
        Value.CopyTo(buffer, offset);
        offset += 256;
        BitConverter.GetBytes(NextRecordAddress).CopyTo(buffer, offset);
    }

    public void SerializeToJson(LiteJson.Json self)
    {
        self["Length"] = Length;
        self["ReferenceCount"] = ReferenceCount;
        self["NextRecordAddress"] = NextRecordAddress;
    }

    public string ToString(bool indented)
    {
        LiteJson.Json self = LiteJson.Json.NewObject();
        SerializeToJson(self);
        return self.ToString(indented);
    }
}
