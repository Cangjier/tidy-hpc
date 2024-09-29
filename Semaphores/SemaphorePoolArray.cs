namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量池集合
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class SemaphorePoolArray<TKey>
    where TKey : notnull
{
    /// <summary>
    /// 初始化信号量池集合
    /// </summary>
    /// <param name="count"></param>
    public SemaphorePoolArray(int count)
    {
        Pools = new SemaphorePool<TKey>[count];
        for (int i = 0; i < count; i++)
        {
            Pools[i] = new SemaphorePool<TKey>();
        }
    }

    private SemaphorePool<TKey>[] Pools { get; }

    /// <summary>
    /// 等待信号量
    /// </summary>
    /// <param name="poolIndex">池索引</param>
    /// <param name="key">信号量key</param>
    /// <returns></returns>
    public async Task WaitAsync(int poolIndex, TKey key)
    {
        await Pools[poolIndex].WaitAsync(key);

    }

    /// <summary>
    /// 释放信号量
    /// </summary>
    /// <param name="poolIndex">池索引</param>
    /// <param name="key">信号量key</param>
    /// <returns></returns>
    public async Task ReleaseAsync(int poolIndex, TKey key)
    {
        await Pools[poolIndex].ReleaseAsync(key);
    }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public SemaphorePool<TKey> this[int index] => Pools[index];
}
