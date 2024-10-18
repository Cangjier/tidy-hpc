using TidyHPC.Queues;

namespace TidyHPC.LiteDB;
/// <summary>
/// 数据库头信息
/// </summary>
public class DatabaseHeader
{
    /// <summary>
    /// 偏移
    /// </summary>
    public static class Offsets
    {
        /// <summary>
        /// 标识符信息的偏移
        /// </summary>
        public const int Flag = 0;
        /// <summary>
        /// 数据库大小的偏移
        /// </summary>
        public const int DatabaseSize = 32;
    }

    /// <summary>
    /// 头大小
    /// </summary>
    public const int Size = Offsets.DatabaseSize + sizeof(long);

    /// <summary>
    /// 获取标识信息
    /// </summary>
    /// <param name="queue"></param>
    /// <returns></returns>
    public async Task<string> GetFlag(FileStreamQueue queue)
    {
        var length = await queue.GetLengthAsync();
        if (length < 32)
        {
            return string.Empty;
        }
        var bytes = await queue.ReadBytesAsync(Offsets.Flag, 32);
        return Util.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 设置标识信息
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public async Task SetFlag(FileStreamQueue queue, string flag)
    {
        await queue.WriteBytesAsync(Offsets.Flag, Util.UTF8.GetBytes(flag));
    }

    /// <summary>
    /// 获取数据库大小
    /// </summary>
    /// <param name="queue"></param>
    /// <returns></returns>
    public async Task<long> GetDatabaseSize(FileStreamQueue queue)
    {
        var length = await queue.GetLengthAsync();
        if (length < Offsets.DatabaseSize+sizeof(long))
        {
            return 0;
        }
        return await queue.ReadLongAsync(Offsets.DatabaseSize);
    }

    /// <summary>
    /// 写入数据库大小
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public async Task SetDatabaseSize(FileStreamQueue queue, long size)
    {
        await queue.WriteLongAsync(Offsets.DatabaseSize, size);
    }

}
