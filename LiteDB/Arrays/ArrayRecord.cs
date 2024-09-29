using TidyHPC.LiteDB.BasicValues;

namespace TidyHPC.LiteDB.Arrays;
/// <summary>
/// 数组记录
/// </summary>
internal struct ArrayRecord<TValue> : IRecord
    where TValue:IValue<TValue>
{
    /// <summary>
    /// 总长度
    /// </summary>
    public int Length;

    /// <summary>
    /// 所有的地址，最大位32，其他放在下一个拼接数组中
    /// </summary>
    public TValue[] Values;

    /// <summary>
    /// 该数组的第一个ArrayRecord地址
    /// </summary>
    public long FirstAddress;

    /// <summary>
    /// 下一个地址
    /// </summary>
    public long NextAddress;

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Read(byte[] buffer, int offset)
    {
        Length = BitConverter.ToInt32(buffer, offset);
        offset += sizeof(int);
        Values = new TValue[32];
        for (int i = 0; i < 32; i++)
        {
            offset += Values[i].Read(buffer, offset);
        }
        FirstAddress = BitConverter.ToInt64(buffer, offset);
        offset += sizeof(long);
        NextAddress = BitConverter.ToInt64(buffer, offset);

    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Write(byte[] buffer, int offset)
    {
        BitConverter.GetBytes(Length).CopyTo(buffer, offset);
        offset += sizeof(int);
        for (int i = 0; i < 32; i++)
        {
            offset += Values[i].Write(buffer, offset);
        }
        BitConverter.GetBytes(FirstAddress).CopyTo(buffer, offset);
        offset += sizeof(long);
        BitConverter.GetBytes(NextAddress).CopyTo(buffer, offset);
    }

    public const int ArrayLength = 32;


    public static int ArraySize = TValue.GetSize() * ArrayLength;

    public static int Size = sizeof(int) + TValue.GetSize() * ArrayLength + sizeof(long) + sizeof(long);

    public static string InterfaceName = $"__ARRAY_RECORD_{typeof(TValue).Name}";
}