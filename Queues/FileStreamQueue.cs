namespace TidyHPC.Queues;

/// <summary>
/// 文件流队列
/// </summary>
public class FileStreamQueue : StreamQueue<FileStream>,IDisposable
{
    /// <summary>
    /// 流数量
    /// </summary>
    public int StreamCount { get; private set; } = 0;

    private bool isDisposed = false;

    /// <summary>
    /// 释放资源，等同于ReleaseResources
    /// </summary>
    public void Dispose()
    {
        if(isDisposed)
        {
            return;
        }
        isDisposed = true;
        ReleaseResources().Wait();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parallelCount"></param>
    public void Open(string path, int parallelCount)
    {
        StreamCount = parallelCount;
        for (int i = 0; i < parallelCount; i++)
        {
            var item = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            Enqueue(item);
        }
    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    /// <returns></returns>
    public override async Task ReleaseResources()
    {
        if (isDisposed) return;
        var streams = await Dequeue(StreamCount);
        foreach (var item in streams)
        {
            await item.DisposeAsync();
        }
        await base.ReleaseResources();
    }
}
