using System.Collections.Concurrent;

namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量场景
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class SemaphoreScene<TKey>
    where TKey : notnull
{
    private SemaphorePool<string> SceneLocker { get; } = new ();

    private ConcurrentDictionary<string, SemaphorePool<TKey>> MapSceneToPool { get; } = new();

    private async Task<SemaphorePool<TKey>> GetPool(string scene)
    {
        await SceneLocker.WaitAsync(scene);
        if (!MapSceneToPool.TryGetValue(scene, out var pool))
        {
            pool = new SemaphorePool<TKey>();
            MapSceneToPool.TryAdd(scene, pool);
        }
        await SceneLocker.ReleaseAsync(scene);
        return pool;
    }

    /// <summary>
    /// 等待信号量
    /// </summary>
    /// <param name="scene">场景key</param>
    /// <param name="key">信号量key</param>
    /// <returns></returns>
    public async Task WaitAsync(string scene, TKey key)
    {
        await (await GetPool(scene)).WaitAsync(key);
        
    }

    /// <summary>
    /// 释放信号量
    /// </summary>
    /// <param name="scene">场景key</param>
    /// <param name="key">信号量key</param>
    /// <returns></returns>
    public async Task ReleaseAsync(string scene, TKey key)
    {
        await (await GetPool(scene)).ReleaseAsync(key);
    }
}
