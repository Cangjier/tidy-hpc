using System.Collections.Concurrent;
using TidyHPC.Loggers;

namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量池
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class TaskCompletionSourcePool<TKey, TValue>
    where TKey : notnull
{
    internal ConcurrentDictionary<TKey, TaskCompletionSource<TValue>> TaskCompletionSources { get; } = new();

    /// <summary>
    /// 添加一个待完成的任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="timeout"></param>
    public async Task<TValue> WaitAsync(TKey id,TimeSpan timeout)
    {
        return await Add(id, timeout).Task;
    }

    /// <summary>
    /// 添加一个待完成的任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public TaskCompletionSource<TValue> Add(TKey id,TimeSpan timeout)
    {
        var completionSource = new TaskCompletionSource<TValue>();
        TaskCompletionSources.TryAdd(id, completionSource);
        _ = Task.Run(async () =>
        {
            await Task.Delay(timeout);
            Cancel(id);
        });
        return completionSource;
    }

    /// <summary>
    /// 取消任务完成
    /// </summary>
    /// <param name="id"></param>
    public void Cancel(TKey id)
    {
        if (TaskCompletionSources.TryRemove(id, out var taskCompletionSource))
        {
            taskCompletionSource.TrySetCanceled();
        }
    }

    /// <summary>
    /// 完成任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="result"></param>
    public void Complete(TKey id, TValue result)
    {
        if (TaskCompletionSources.TryRemove(id, out var taskCompletionSource))
        {
            if (taskCompletionSource.TrySetResult(result)==false)
            {
                Logger.Error("TaskCompletionSourcePool Complete Error");
            }
        }
    }

    /// <summary>
    /// 是否包含某个任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Contains(TKey id)
    {
        return TaskCompletionSources.ContainsKey(id);
    }
}