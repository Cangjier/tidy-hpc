namespace TidyHPC.LiteDB.Layouts.Visitors;
/// <summary>
/// 记录区域的访问器
/// </summary>
/// <param name="block"></param>
public class RecordRegionVisitor(Layout block)
{
    /// <summary>
    /// 访问的块
    /// </summary>
    public Layout Block { get; } = block;

    /// <summary>
    /// 读取记录区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, Action<byte[]> onBuffer)
        => await LayoutUtil.ProcessRead(table, Block.FirstRecordAddress, Block.RecordRegionSize, table.RecordSemaphore, onBuffer);

    /// <summary>
    /// 读取记录区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, Func<byte[], Task> onBuffer)
        => await LayoutUtil.ProcessRead(table, Block.FirstRecordAddress, Block.RecordRegionSize, table.RecordSemaphore, onBuffer);

    /// <summary>
    /// 写入记录区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, Action<byte[]> onBuffer)
        => await LayoutUtil.ProcessWrite(table, Block.FirstRecordAddress, Block.RecordRegionSize, table.RecordSemaphore, onBuffer);

    /// <summary>
    /// 更新记录区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, Func<byte[], bool> onBuffer)
        => await LayoutUtil.ProcessUpdate(table, Block.FirstRecordAddress, Block.RecordRegionSize, table.RecordSemaphore, onBuffer);

    /// <summary>
    /// 更新记录区域
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, Func<byte[], Task<bool>> onBuffer)
        => await LayoutUtil.ProcessUpdate(table, Block.FirstRecordAddress, Block.RecordRegionSize, table.RecordSemaphore, onBuffer);
}
