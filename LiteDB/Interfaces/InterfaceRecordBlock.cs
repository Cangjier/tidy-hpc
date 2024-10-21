using TidyHPC.LiteDB.Layouts;
using TidyHPC.LiteDB.Metas2;

namespace TidyHPC.LiteDB.Interfaces;

/// <summary>
/// 接口记录块
/// </summary>
public class InterfaceRecordBlock:StatisticalLayout
{
    /// <summary>
    /// 块大小
    /// </summary>
    public const int Size = 8192;

    /// <summary>
    /// 接口记录块
    /// </summary>
    /// <param name="address"></param>
    public InterfaceRecordBlock(long address)
    {
        SetAddress(address, InterfaceRecord.Size, Size);
    }

}
