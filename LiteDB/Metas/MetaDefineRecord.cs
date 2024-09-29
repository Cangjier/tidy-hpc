namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 用于描述元数据具体信息的记录
/// </summary>
internal struct MetaDefineRecord:IRecord
{
    public void Read(byte[] buffer, int offset)
    {
        FieldCount = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        FieldNames = new long[256];
        for (int i = 0; i < 256; i++)
        {
            FieldNames[i] = BitConverter.ToInt64(buffer, offset);
            offset += sizeof(long);
        }
        FieldTypes = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            FieldTypes[i] = buffer[offset];
            offset++;
        }
        FieldArrayLengths = new int[256];
        for (int i = 0; i < 256; i++)
        {
            FieldArrayLengths[i] = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);
        }
        FieldMapTypes = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            FieldMapTypes[i] = buffer[offset];
            offset++;
        }
        FieldMapAddresses = new long[256];
        for (int i = 0; i < 256; i++)
        {
            FieldMapAddresses[i] = BitConverter.ToInt64(buffer, offset);
            offset += sizeof(long);
        }
    }

    /// <summary>
    /// 字段数量
    /// </summary>
    public int FieldCount;

    /// <summary>
    /// 字段名称，字符串从StringHashSet中获取
    /// </summary>
    public long[] FieldNames;

    /// <summary>
    /// 字段类型
    /// </summary>
    public byte[] FieldTypes;

    /// <summary>
    /// 字段数组长度
    /// </summary>
    public int[] FieldArrayLengths;

    /// <summary>
    /// 字段映射类型
    /// </summary>
    public byte[] FieldMapTypes;

    /// <summary>
    /// 映射地址
    /// </summary>
    public long[] FieldMapAddresses;

    /// <summary>
    /// 大小
    /// </summary>
    public const int Size = sizeof(int) + (sizeof(byte) * 2 + sizeof(int) + sizeof(long) * 2) * 256;

    /// <summary>
    /// 将数据写入到buffer中
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public readonly void Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(FieldCount).CopyTo(buffer, offset);
        offset += sizeof(int);
        for (int i = 0; i < 256; i++)
        {
            BitConverter.GetBytes(FieldNames[i]).CopyTo(buffer, offset);
            offset += sizeof(long);
        }
        for (int i = 0; i < 256; i++)
        {
            buffer[offset] = FieldTypes[i];
            offset++;
        }
        for (int i = 0; i < 256; i++)
        {
            BitConverter.GetBytes(FieldArrayLengths[i]).CopyTo(buffer, offset);
            offset += sizeof(int);
        }
        for (int i = 0; i < 256; i++)
        {
            buffer[offset] = FieldMapTypes[i];
            offset++;
        }
        for (int i = 0; i < 256; i++)
        {
            BitConverter.GetBytes(FieldMapAddresses[i]).CopyTo(buffer, offset);
            offset += sizeof(long);
        }
    }
}
