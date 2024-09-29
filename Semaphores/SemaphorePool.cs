using System.Collections.Concurrent;

namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量池
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class SemaphorePool<TKey>: ISemaphorePool<TKey>
    where TKey : notnull
{
    private class SemaphoreItem(int initialCount, int maxCount)
    {
        public int ReferenceCount = 0;

        public SemaphoreSlim Target { get; } = new(initialCount, maxCount);
    }

    /// <summary>
    /// 信号量池
    /// </summary>
    public SemaphorePool()
    {

    }

    /// <summary>
    /// 信号量池
    /// </summary>
    /// <param name="initialCount">每个信号量初始数量</param>
    /// <param name="maxCount">每个信号量最大数量</param>
    public SemaphorePool(int initialCount, int maxCount)
    {
        InitialCount = initialCount;
        MaxCount = maxCount;
    }

    /// <summary>
    /// 每个信号量初始数量
    /// </summary>
    public int InitialCount { get; } = 1;

    /// <summary>
    /// 每个信号量最大数量
    /// </summary>
    public int MaxCount { get; } = 1;

    private ConcurrentDictionary<TKey, SemaphoreItem> RegisterMap { get; } = new();

    private ConcurrentQueue<SemaphoreItem> AvailableSemaphores { get; } = new();

    private SemaphoreSlim AvailableSlim { get; } = new(1, 1);

    private async Task<SemaphoreItem> GetOrCreate(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (!RegisterMap.TryGetValue(key, out SemaphoreItem? slim))
        {
            if(!AvailableSemaphores.TryDequeue(out slim))
            {
                slim = new(InitialCount, MaxCount);
            }
            RegisterMap[key] = slim;
        }
        slim.ReferenceCount++;
        AvailableSlim.Release();
        return slim;
    }

    /// <summary>
    /// 等待信号量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task WaitAsync(TKey key)
    {
        await (await GetOrCreate(key)).Target.WaitAsync();
    }

    /// <summary>
    /// 释放信号量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task ReleaseAsync(TKey key)
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
            slim.Target.Release();
            if(isAvailable)
            {
                AvailableSemaphores.Enqueue(slim);
            }
        }
        AvailableSlim.Release();
    }
}
