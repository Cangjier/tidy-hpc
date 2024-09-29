namespace TidyHPC.Semaphores;

/// <summary>
/// 读写信号量
/// </summary>
public class ReaderWriterSemaphore
{
    /// <summary>
    /// 支持的最大读者数量
    /// </summary>
    /// <param name="readerCount"></param>
    public ReaderWriterSemaphore(int readerCount)
    {
        ReaderCount = readerCount;
        ReaderSemaphores = new(readerCount);
    }

    private int ReaderCount { get; }

    private SemaphoreSlim ReaderSemaphores { get; }

    private SemaphoreSlim WriterSemaphore { get; } = new(1);

    /// <summary>
    /// 开始读
    /// </summary>
    /// <returns></returns>
    public async Task BeginRead()
    {
        await WriterSemaphore.WaitAsync();
        WriterSemaphore.Release();
        await ReaderSemaphores.WaitAsync();
    }

    /// <summary>
    /// 结束读
    /// </summary>
    /// <returns></returns>
    public void EndRead()
    {
        ReaderSemaphores.Release();
    }

    /// <summary>
    /// 开始写
    /// </summary>
    /// <returns></returns>
    public async Task BeginWrite()
    {
        await WriterSemaphore.WaitAsync();
        for (int i = 0; i < ReaderCount; i++)
        {
            await ReaderSemaphores.WaitAsync();
        }
    }

    /// <summary>
    /// 结束写
    /// </summary>
    /// <returns></returns>
    public void EndWrite()
    {
        for (int i = 0; i < ReaderCount; i++)
        {
            ReaderSemaphores.Release();
        }
        WriterSemaphore.Release();
    }
}
