namespace TidyHPC.LiteDB.Metas2;

/// <summary>
/// 类型记录
/// </summary>
public struct InterfaceRecord : IRecord
{
    /// <summary>
    /// 完全限定名，最长256
    /// </summary>
    public string FullName;

    /// <summary>
    /// 约定大小
    /// </summary>
    public int InterfaceSize;

    /// <summary>
    /// 仓库
    /// </summary>
    public long Repositories;

    /// <summary>
    /// 定义
    /// </summary>
    public long Define;

    /// <summary>
    /// 接口记录的大小
    /// </summary>
    public const int Size = 256 + sizeof(int) + sizeof(long) + sizeof(long);

    /// <inheritdoc/>
    public void Read(byte[] buffer, int offset)
    {
        FullName = buffer.DeserializeString(ref offset, 256);
        InterfaceSize = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        Repositories = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
        Define = BitConverter.ToInt64(buffer, offset);
    }

    /// <inheritdoc/>
    public void Write(byte[] buffer, int offset)
    {
        buffer.SerializeRef(FullName, ref offset, 256);
        BitConverter.GetBytes(InterfaceSize).CopyTo(buffer, offset);
        offset += sizeof(int);
        BitConverter.GetBytes(Repositories).CopyTo(buffer, offset);
        offset += sizeof(long);
        BitConverter.GetBytes(Define).CopyTo(buffer, offset);
    }

    /// <summary>
    /// Parse
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static InterfaceRecord Parse(byte[] buffer)
    {
        var record = new InterfaceRecord();
        record.Read(buffer, 0);
        return record;
    }
}
