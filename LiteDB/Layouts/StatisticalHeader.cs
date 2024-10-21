namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 统计头部
/// </summary>
public class StatisticalHeader: LayoutHeader<StatisticalHeader>
{
    /// <summary>
    /// 统计数据
    /// </summary>
    public required byte[] StatisticalData;

    /// <summary>
    /// 
    /// </summary>
    public required StatisticalLayout Layout { get; set; }

    /// <summary>
    /// 读取统计区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, Action<byte[], int> onBuffer)
    {
        await LayoutUtil.ProcessReadWithCache(
        table,
        Block.Address,
        Block.CacheSize,
        Block.StatisticalRegionAddress,
        Block.StatisticalRegionSize,
        table.StatisticalSemaphore,
        onBuffer);
    }

    /// <summary>
    /// 写入统计区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, Action<byte[], int> onBuffer)
    {
        await LayoutUtil.ProcessWriteWithCache(
        table,
        Block.Address,
        Block.CacheSize,
        Block.StatisticalRegionAddress,
        Block.StatisticalRegionSize,
        table.StatisticalSemaphore,
        onBuffer);
    }

    /// <summary>
    /// 更新统计区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, Func<byte[], int, bool> onBuffer)
    {
        await LayoutUtil.ProcessUpdateWithCache(
        table,
        Block.Address,
        Block.CacheSize,
        Block.StatisticalRegionAddress,
        Block.StatisticalRegionSize,
        table.StatisticalSemaphore,
        onBuffer);
    }
}
