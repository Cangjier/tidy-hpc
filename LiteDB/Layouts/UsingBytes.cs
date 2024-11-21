namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 使用的字节
/// </summary>
public class UsingBytes : IDisposable
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="bytes"></param>
    public UsingBytes(BytesCache cache, byte[] bytes)
    {
        Cache = cache;
        Bytes = bytes;
    }

    /// <summary>
    /// 缓存对象
    /// </summary>
    public BytesCache Cache { get; }

    /// <summary>
    /// 字节
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// 归还
    /// </summary>
    public void Dispose()
    {
        Cache.EnqueueBuffer(Bytes);
    }
}
