namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量池接口
/// </summary>
public interface ISemaphorePool<TKey>
    where TKey : notnull
{
    /// <summary>
    /// 等待信号量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task WaitAsync(TKey key);

    /// <summary>
    /// 释放信号量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task ReleaseAsync(TKey key);
}
