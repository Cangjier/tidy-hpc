using System.Collections.Concurrent;

namespace TidyHPC.Locks;
internal class LockPool<TKey,TValue>(Func<TValue> onNew)
    where TKey : notnull
{
    private class Item(TValue value)
    {
        public int ReferenceCount = 0;

        public Lock<TValue> Target { get; } = new(value);
    }

    private ConcurrentDictionary<TKey, Item> RegisterMap { get; } = new();

    private ConcurrentQueue<Item> AvailablePool { get; } = new();

    private SemaphoreSlim AvailableSlim { get; } = new(1, 1);

    private Func<TValue> OnNew { get; } = onNew;

    private async Task<Item> GetOrCreate(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (!RegisterMap.TryGetValue(key, out Item? slim))
        {
            if (!AvailablePool.TryDequeue(out slim))
            {
                slim = new(OnNew());
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
    public async Task<Lock<TValue>> Begin(TKey key)
    {
        return (await GetOrCreate(key)).Target;
    }

    /// <summary>
    /// 释放信号量
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task End(TKey key)
    {
        await AvailableSlim.WaitAsync();
        if (RegisterMap.TryGetValue(key, out Item? slim))
        {
            slim.ReferenceCount--;
            bool isAvailable = slim.ReferenceCount == 0;
            if (isAvailable)
            {
                RegisterMap.TryRemove(key, out _);
            }
            if (isAvailable)
            {
                AvailablePool.Enqueue(slim);
            }
        }
        AvailableSlim.Release();
    }

    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(TKey key)
    {
        return RegisterMap.ContainsKey(key);
    }
}
