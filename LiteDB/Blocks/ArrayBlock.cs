namespace TidyHPC.LiteDB.Blocks;

/// <summary>
/// Represent a block that contains an array of records
/// </summary>
public class ArrayBlock : Block
{
    /// <summary>
    /// 初始化块的数据
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public override async Task Initialize(Database table)
    {
        await table.FileStream.WriteAsync(Address, new byte[BlockSize], 0, BlockSize);
    }

    /// <summary>
    /// 设置块的必要信息
    /// </summary>
    /// <param name="address"></param>
    /// <param name="recordSize"></param>
    /// <param name="blockSize"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void SetAddress(long address, int recordSize, int blockSize)
    {
        Address = address;
        RecordSize = recordSize;
        BlockSize = blockSize;

        RecordCount = blockSize / recordSize;
        FirstRecordAddress = address;
        RecordRegionSize = recordSize * RecordCount;
    }
}
