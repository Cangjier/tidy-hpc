namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 字节缓存配置
/// </summary>
public class BytesCacheConfig
{
    /// <summary>
    /// 大字节缓存数量
    /// </summary>
    public int BigBytesCacheCount = 8;

    /// <summary>
    /// 小字节缓存数量
    /// </summary>
    public int SmallBytesCacheCount = 32;
}
