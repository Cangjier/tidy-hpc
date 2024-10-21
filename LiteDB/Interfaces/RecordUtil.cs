namespace TidyHPC.LiteDB.Interfaces;

/// <summary>
/// 记录工具
/// </summary>
/// <param name="database"></param>
public class RecordUtil(Database database)
{
    /// <summary>
    /// 数据库
    /// </summary>
    public Database Database { get; } = database;

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="recordAddress"></param>
    /// <param name="recordSize"></param>
    /// <param name="blockSize"></param>
    /// <returns></returns>
    public async Task<T> Read<T>(long recordAddress,int recordSize,int blockSize)
        where T : IRecord, new()
    {
        var blockAddress = await Database.FileStream.ReadLongAsync(recordAddress);
        T record = new();
        await Database.Cache.StatisticalBlockPool.Use(async block =>
        {
            block.SetAddress(blockAddress, recordSize, blockSize);
            await block.RecordVisitor.Read(Database, recordAddress, bytes =>
            {
                record.Read(bytes, 0);
            });
        });
        return record;
    }
}
