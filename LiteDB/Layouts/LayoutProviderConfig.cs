namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 提供者参数
/// </summary>
public class LayoutProviderConfig
{
    /// <summary>
    /// 记录读取者信号量最大数
    /// </summary>
    public int RecordReaderSemaphoreMaxCount = 8;

    public BytesCacheConfig BytesCacheConfig { get; } = new ();

}
