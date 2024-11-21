namespace TidyHPC.LiteDB.Layouts.Visitors;

/// <summary>
/// 记录访问器
/// </summary>
/// <param name="block"></param>
public class RecordVisitor(Layout block)
{
    /// <summary>
    /// 访问的块
    /// </summary>
    public Layout Block { get; } = block;

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(LayoutProvider provider, long address, Action<byte[]> onBuffer)
    {
        await LayoutUtil.ProcessRead(provider, address, Block.Properties.RecordSize, provider.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(LayoutProvider provider, long address, Func<byte[], Task> onBuffer)
    {
        await LayoutUtil.ProcessRead(provider, address, Block.Properties.RecordSize, provider.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task<TResult> Read<TResult>(Database table, long address, Func<byte[], Task<TResult>> onBuffer)
    {
        return await LayoutUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task<TResult> Read<TResult>(Database table, long address, Func<byte[], TResult> onBuffer)
    {
        return await LayoutUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task ReadByIndex(Database table, int index, Action<byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await LayoutUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task ReadByIndex(Database table, int index, Action<long, byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await LayoutUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, buffer => onBuffer(address, buffer));
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, long address, Action<byte[]> onBuffer)
    {
        await LayoutUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, long address, Func<byte[], Task> onBuffer)
    {
        await LayoutUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteSpan(Database table, long address, int spanOffset, int spanSize, Action<byte[]> onBuffer)
    {
        await LayoutUtil.ProcessWriteSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteSpan(Database table, long address, int spanOffset, int spanSize, Func<byte[], Task> onBuffer)
    {
        await LayoutUtil.ProcessWriteSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteByIndex(Database table, int index, Action<byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await LayoutUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, long address, Func<byte[], bool> onBuffer)
    {
        await LayoutUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, long address, Func<byte[], Task<bool>> onBuffer)
    {
        await LayoutUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <returns></returns>
    public async Task UpdateSpan(Database table, long address, int spanOffset, int spanSize, Func<byte[], Task<bool>> onBuffer)
    {
        await LayoutUtil.ProcessUpdateSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <returns></returns>
    public async Task UpdateSpan(Database table, long address, int spanOffset, int spanSize, Func<byte[], bool> onBuffer)
    {
        await LayoutUtil.ProcessUpdateSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task UpdateByIndex(Database table, int index, Func<byte[], bool> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await LayoutUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task UpdateByIndex(Database table, int index, Func<byte[], Task<bool>> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await LayoutUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }
}
