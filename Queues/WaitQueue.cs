using System.Collections.Concurrent;

namespace TidyHPC.Queues;

/// <summary>
/// 等待队列的接口
/// </summary>
public interface IWaitQueue
{
    /// <summary>
    /// 向队列中添加值，并通知等待队列的处理者
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public object Enqueue(object value);

    /// <summary>
    /// 从队列中取出值，如果队列为空，则等待
    /// </summary>
    /// <returns></returns>
    public Task<object> Dequeue();
}

/// <summary>
/// 等待队列
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class WaitQueue<TValue>: IWaitQueue
{
    /// <summary>
    /// 等待队列
    /// </summary>
    public WaitQueue()
    {
        WaitQueueSemaphore = new SemaphoreSlim(0);
    }

    /// <summary>
    /// 等待队列，初始化值
    /// </summary>
    /// <param name="initialValues"></param>
    public WaitQueue(List<TValue> initialValues)
    {
        WaitQueueSemaphore = new SemaphoreSlim(0);
        initialValues.ForEach(item=> Enqueue(item));
    }

    private ConcurrentQueue<TValue> Target { get; } = new();

    private SemaphoreSlim WaitQueueSemaphore { get; }

    /// <summary>
    /// 当前队列长度
    /// </summary>
    public int CurrentCount => Target.Count;

    /// <summary>
    /// 当队列中有值待被取出时触发，可以用于通知生产者
    /// <para>该操作不会对Dequeue造成阻塞</para>
    /// </summary>
    public Func<Task>? OnDequeueStart { get; set; } = null;

    /// <summary>
    /// 当队列中有值已被取出时触发，可以用于通知生产者
    /// <para>该操作不会对Dequeue造成阻塞</para>
    /// </summary>
    public Func<TValue?,Task>? OnDequeue { get; set; } = null;

    private void NotifyWaitQueue()
    {
        WaitQueueSemaphore.Release();
    }

    /// <summary>
    /// 向队列中添加值，并通知等待队列的处理者
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public TValue Enqueue(TValue value)
    {
        Target.Enqueue(value);
        NotifyWaitQueue();
        return value;
    }

    object IWaitQueue.Enqueue(object value)
    {
        return Enqueue((TValue)value) ?? throw new NullReferenceException();
    }

    /// <summary>
    /// 初始化队列值
    /// </summary>
    /// <param name="count"></param>
    /// <param name="onNew"></param>
    /// <returns></returns>
    public WaitQueue<TValue> Initialize(int count,Func<TValue> onNew)
    {
        for (int i = 0; i < count; i++)
        {
            Enqueue(onNew());
        }
        return this;
    }

    /// <summary>
    /// 从队列中取出值，如果队列为空，则等待
    /// </summary>
    /// <returns></returns>
    public async Task<TValue> Dequeue()
    {
        OnDequeueStart?.Invoke();
        await WaitQueueSemaphore.WaitAsync();
        Target.TryDequeue(out var result);
        OnDequeue?.Invoke(result);
        if (result == null)
        {
            throw new Exception("Dequeue result is null");
        }
        return result;
    }

    async Task<object> IWaitQueue.Dequeue()
    {
        return await Dequeue() ?? throw new NullReferenceException();
    }

    /// <summary>
    /// 取出当前队列所有的值
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IEnumerable<TValue>> DequeueAll()
    {
        var result = new List<TValue>();
        while (CurrentCount > 0)
        {
            var item = await Dequeue();
            result.Add(item);
        }
        return result;
    }

    /// <summary>
    /// 取出指定数量的值
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TValue>> Dequeue(int count)
    {
        var result = new List<TValue>();
        while (count > 0)
        {
            result.Add(await Dequeue());
            count--;
        }
        return result;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <returns></returns>
    public virtual async Task ReleaseResources()
    {
        WaitQueueSemaphore.Dispose();
        Target.Clear();
        await Task.CompletedTask;
    }

    /// <summary>
    /// 使用队列中的值，并在使用完成后将值重新放回队列
    /// </summary>
    /// <param name="onUse"></param>
    /// <returns></returns>
    public virtual async Task Use(Func<TValue,Task> onUse)
    {
        var item = await Dequeue();
        try
        {
            await onUse(item);
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(item);
        }
    }

    /// <summary>
    /// 使用队列中的值，并在使用完成后将值重新放回队列
    /// </summary>
    /// <param name="onUse"></param>
    /// <returns></returns>
    public virtual async Task<TResult> Use<TResult>(Func<TValue, Task<TResult>> onUse)
    {
        var item = await Dequeue();
        try
        {
            return await onUse(item);
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(item);
        }
    }

    /// <summary>
    /// 使用队列中的值，并在使用完成后将值重新放回队列
    /// </summary>
    /// <param name="onUse"></param>
    /// <returns></returns>
    public virtual async Task Use(Func<Task> onUse)
    {
        var result = await Dequeue();
        try
        {
            await onUse();
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(result);
        }
    }
}
