using TidyHPC.LiteJson;

namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 元数据记录
/// </summary>
public struct MetaRecord:IRecord
{
    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Read(byte[] buffer, int offset)
    {
        TypeName = buffer.DeserializeString(ref offset, 256);
        RecordSize = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        DefineRecordAddress = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
        BlockAddresses = new long[1024];
        Buffer.BlockCopy(buffer, offset, BlockAddresses, 0, sizeof(long) * 1024);
        offset += sizeof(long) * 1024;
        NextMetaRecordAddress = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
        FirstMetaRecordAddress = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
    }

    /// <summary>
    /// Read data
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="database"></param>
    public void Read(byte[] buffer, int offset,Database database)
    {
        Read(buffer, offset);
        //database.DebugLogger.WriteLine($"Read {TypeName},{RecordSize},{DefineRecordAddress},{new Json(BlockAddresses.Where(item => item != 0).ToArray()).ToString(false)},{NextMetaRecordAddress},{FirstMetaRecordAddress}");
    }

    /// <summary>
    /// 类型名称，完全限定名，最长256
    /// </summary>
    public string TypeName;

    /// <summary>
    /// 记录的大小
    /// </summary>
    public int RecordSize;

    /// <summary>
    /// 定义记录的地址
    /// </summary>
    public long DefineRecordAddress;

    /// <summary>
    /// 块的地址，1024
    /// </summary>
    public long[] BlockAddresses;

    /// <summary>
    /// 下一个Meta记录的地址
    /// </summary>
    public long NextMetaRecordAddress;

    /// <summary>
    /// 第一个Meta记录的地址
    /// </summary>
    public long FirstMetaRecordAddress;

    /// <summary>
    /// 该记录的大小
    /// </summary>
    public const int Size = sizeof(byte) * 256 + sizeof(int) + sizeof(long) + sizeof(long) * 1024 + sizeof(long) + sizeof(long);

    /// <summary>
    /// 将数据写入到buffer中
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Write(byte[] buffer, int offset)
    {
        buffer.SerializeRef(TypeName, ref offset, 256);
        BitConverter.GetBytes(RecordSize).CopyTo(buffer, offset);
        offset += sizeof(int);
        BitConverter.GetBytes(DefineRecordAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
        Buffer.BlockCopy(BlockAddresses, 0, buffer, offset, sizeof(long) * 1024);
        offset += sizeof(long) * 1024;
        BitConverter.GetBytes(NextMetaRecordAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
        BitConverter.GetBytes(FirstMetaRecordAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
    }

    /// <summary>
    /// Write data
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="database"></param>
    public void Write(byte[] buffer, int offset, Database database)
    {
        Write(buffer, offset);
        //database.DebugLogger.WriteLine($"Write {TypeName},{RecordSize},{DefineRecordAddress},{new Json(BlockAddresses.Where(item => item != 0).ToArray()).ToString(false)},{NextMetaRecordAddress},{FirstMetaRecordAddress}");
    }
}
