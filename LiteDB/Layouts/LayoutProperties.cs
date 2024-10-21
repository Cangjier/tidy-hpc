namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 布局属性
/// </summary>
public class LayoutProperties
{
    /// <summary>
    /// 块的首地址
    /// </summary>
    public long Address;

    /// <summary>
    /// 头大小
    /// </summary>
    public int HeaderSize;

    /// <summary>
    /// 单个记录的大小
    /// </summary>
    public int RecordSize;

    /// <summary>
    /// 块的大小
    /// </summary>
    public int LayoutSize;

    /// <summary>
    /// 记录的数量是通过块的大小计算的
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// 第一个记录的地址
    /// </summary>
    public long FirstRecordAddress { get; set; }

    /// <summary>
    /// 记录区域的大小
    /// </summary>
    public int RecordRegionSize { get; set; }

    /// <summary>
    /// Address + BlockSize 
    /// </summary>
    public long BoundarySize { get; set; }

    /// <summary>
    /// 设置布局属性
    /// </summary>
    /// <param name="address"></param>
    /// <param name="layoutSize"></param>
    /// <param name="headerSize"></param>
    /// <param name="recordSize"></param>
    public void Set(long address, int layoutSize,int headerSize, int recordSize)
    {
        Address = address;
        HeaderSize = headerSize;
        RecordSize = recordSize;
        LayoutSize = layoutSize;

        RecordCount = (layoutSize - headerSize) / recordSize;
        FirstRecordAddress = address + headerSize;
        RecordRegionSize = recordSize * RecordCount;
        BoundarySize = address + layoutSize;
    }
}
