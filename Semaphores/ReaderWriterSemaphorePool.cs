using System.Collections.Concurrent;

namespace TidyHPC.Semaphores;

/// <summary>
/// 读写信号量池
/// <para>`读`与`读`是可以并行的</para>
/// <para>`读`与`写`是互斥的，开始写，会等待所有`读`完成，`写`时，所有读都会等待`写`完成</para>
/// <para>`写`与`写`是互斥的</para>
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class ReaderWriterSemaphorePool<TKey>
    where TKey : notnull
{
    private class SemaphoreItem(int readerCount)
    {
        public int ReferenceCount = 0;

        public ReaderWriterSemaphore Target { get; } = new(readerCount);
    }

    /// <summary>
    /// 信号量池
    /// </summary>
    /// <param name="readerCount">每个信号量读初始数量</param>
    public ReaderWriterSemaphorePool(int readerCount)
    {
        ReaderCount = readerCount;
    }

    /// <summary>
    /// 读信号量初始数量
    /// </summary>
    public int ReaderCount { get; } 

    private ConcurrentDictionary<TKey, SemaphoreItem> RegisterMap { get; } = new();

    private ConcurrentQueue<SemaphoreItem> AvailableSemaphores { get; } = new();

    private SemaphoreSlim AvailableSlim { get; } = new(1, 1);

    private async Task<SemaphoreItem> GetOrCreate(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (!RegisterMap.TryGetValue(key, out SemaphoreItem? slim))
        {
            if (!AvailableSemaphores.TryDequeue(out slim))
            {
                slim = new(ReaderCount);
            }
            RegisterMap[key] = slim;
        }
        slim.ReferenceCount++;
        AvailableSlim.Release();
        return slim;
    }

    /// <summary>
    /// 开始读
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task BeginRead(TKey key)
    {
        await (await GetOrCreate(key)).Target.BeginRead();
    }

    /// <summary>
    /// 结束读
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task EndRead(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (RegisterMap.TryGetValue(key, out SemaphoreItem? slim))
        {
            slim.ReferenceCount--;
            bool isAvailable = slim.ReferenceCount == 0;
            if (isAvailable)
            {
                RegisterMap.TryRemove(key, out _);
            }
            slim.Target.EndRead();
            if (isAvailable)
            {
                AvailableSemaphores.Enqueue(slim);
            }
        }
        AvailableSlim.Release();
    }

    /// <summary>
    /// 开始写
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task BeginWrite(TKey key)
    {
        await (await GetOrCreate(key)).Target.BeginWrite();
    }

    /// <summary>
    /// 结束写
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task EndWrite(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (RegisterMap.TryGetValue(key, out SemaphoreItem? slim))
        {
            slim.ReferenceCount--;
            bool isAvailable = slim.ReferenceCount == 0;
            if (isAvailable)
            {
                RegisterMap.TryRemove(key, out _);
            }
            slim.Target.EndWrite();
            if (isAvailable)
            {
                AvailableSemaphores.Enqueue(slim);
            }
        }
        AvailableSlim.Release();
    }
}