using TidyHPC.LiteDB.Caches;
using TidyHPC.Queues;
using TidyHPC.Semaphores;

namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 布局数据提供者
/// </summary>
public class LayoutProvider
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="config"></param>
    public LayoutProvider(LayoutProviderConfig config)
    {
        Config = config;
        RecordSemaphore = new(Config.RecordReaderSemaphoreMaxCount);
        Cache = new(this);
    }

    /// <summary>
    /// 配置
    /// </summary>
    public LayoutProviderConfig Config { get; }

    /// <summary>
    /// 记录的信号量
    /// </summary>
    public ReaderWriterSemaphorePool<long> RecordSemaphore { get; }

    /// <summary>
    /// 缓存
    /// </summary>
    public Cache Cache { get;  }

    /// <summary>
    /// 文件流
    /// </summary>
    public FileStreamQueue FileStream { get; } = new();
}
