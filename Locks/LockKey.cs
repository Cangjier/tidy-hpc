using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace TidyHPC.Locks;

/// <summary>
/// 基于Key的锁
/// </summary>
public class LockKey
{
    private class LockCounter(object lockObject)
    {
        public int Count = 1;
        public object LockObject = lockObject ?? throw new ArgumentNullException(nameof(lockObject));
    }

    private readonly Dictionary<Guid, LockCounter> _lockMap = new();
    private readonly Queue<LockCounter> _lockPool = new();
    private readonly object _lockMapLock = new();

    /// <summary>
    /// 处理锁
    /// </summary>
    /// <param name="key">锁的唯一标识</param>
    /// <param name="action">需要加锁执行的逻辑</param>
    /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
    public void Process(Guid key, Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        LockCounter? counter;
        lock (_lockMapLock)
        {
            if (!_lockMap.TryGetValue(key, out counter))
            {
                if (!_lockPool.TryDequeue(out var pooledCounter))
                {
                    pooledCounter = new LockCounter(new object());
                }
                else
                {
                    pooledCounter.Count = 1;
                }
                counter = pooledCounter;
                _lockMap[key] = counter;
            }
            else
            {
                counter.Count++;
            }
        }

        try
        {
            if (counter?.LockObject == null)
                throw new InvalidOperationException("LockObject is null");

            lock (counter.LockObject)
            {
                action();
            }
        }
        finally
        {
            lock (_lockMapLock)
            {
                counter.Count--;
                if (counter.Count == 0)
                {
                    _lockMap.Remove(key);
                    _lockPool.Enqueue(counter);
                }
            }
        }
    }
}