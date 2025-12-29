using System;
using System.Collections.Concurrent;
using TidyHPC.Semaphores;


namespace TidyHPC.Queues;

/// <summary>
/// 多任务处理器队列
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class MultiplyTaskProcessorQueue<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// 多任务处理器队列
    /// </summary>
    /// <param name="processorCount"></param>
    /// <param name="processor"></param>
    /// <param name="onDequeue"></param>
    public MultiplyTaskProcessorQueue(int processorCount, IProcessor<TValue> processor, Func<TValue?, Task>? onDequeue = null)
    {
        OnDequeue = onDequeue;
        ProcessorVisitor = new ProcessorVisitor<TValue>(processorCount, processor);
    }
    
    private class TaskQueueItem(TKey key, ProcessorVisitor<TValue> processorVisitor, Action<TaskQueueItem> onTaskComplete)
    {
        public TKey Key { get; } = key;

        public ProcessorVisitor<TValue> ProcessorVisitor { get; set; } = processorVisitor;

        public Action<TaskQueueItem> OnTaskComplete = onTaskComplete;

        public Func<TValue?, Task>? OnDequeue { get; set; } = null;

        public WaitQueue<TValue> TaskQueue { get; set; } = new();
        public ReverseSemaphore TaskEmptySemaphore { get; set; } = new();
        public CancellationTokenSource Cancellation { get; set; } = new();
        public bool IsRemoveFromParentWhenEmpty { get; set; } = false;

        /// <summary>
        /// 是否在取消时释放资源
        /// </summary>
        public bool IsReleaseResourcesWhenCancel { get; set; } = true;

        private bool IsReleased { get; set; } = false;


        private SemaphoreSlim? ConcurrentSemaphore { get; set; } = null;

        private int Concurrent { get; set; } = 0;

        /// <summary>
        /// 设置并发数量
        /// </summary>
        /// <param name="concurrent"></param>
        public void SetConcurrent(int concurrent)
        {
            if (ConcurrentSemaphore == null)
            {
                ConcurrentSemaphore = new SemaphoreSlim(concurrent);
                Concurrent = concurrent;
            }
            else if (Concurrent != concurrent)
            {
                if (concurrent > Concurrent)
                {
                    ConcurrentSemaphore.Release(concurrent - Concurrent);
                }
                else
                {
                    for (int i = 0; i < Concurrent - concurrent; i++)
                    {
                        _ = ConcurrentSemaphore.WaitAsync();
                    }
                }
                Concurrent = concurrent;
            }

        }


        /// <summary>
        /// 释放资源
        /// </summary>
        public async Task ReleaseResources()
        {
            if (IsReleased) return;
            IsReleased = true;
            Cancellation.Dispose();
            await TaskQueue.ReleaseResources();
            ProcessorVisitor = null!;
            OnTaskComplete = null!;
            OnDequeue = null!;
            TaskEmptySemaphore.Dispose();
            TaskEmptySemaphore = null!;
            ConcurrentSemaphore?.Dispose();
            ConcurrentSemaphore = null!;
            Concurrent = 0;
        }


        /// <summary>
        /// 启动处理
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Start()
        {
            while (true)
            {
                if (Cancellation.IsCancellationRequested)
                {
                    if (IsReleaseResourcesWhenCancel)
                    {
                        await ReleaseResources();
                    }
                    break;
                }
                var task = await TaskQueue.Dequeue();
                if (Cancellation.IsCancellationRequested)
                {
                    if (IsReleaseResourcesWhenCancel)
                    {
                        await ReleaseResources();
                    }
                    break;
                }
                var processor = await ProcessorVisitor.Dequeue();
                if (ConcurrentSemaphore != null)
                {
                    await ConcurrentSemaphore.WaitAsync();
                }
                _ = Task.Run(async () =>
                {
                    await processor.Process(task);
                    TaskEmptySemaphore.Decrement();
                    ProcessorVisitor.Enqueue();
                    OnTaskComplete(this);
                });
                if (ConcurrentSemaphore != null)
                {
                    ConcurrentSemaphore.Release();
                }
            }
        }
    }

    private ConcurrentDictionary<TKey, TaskQueueItem> TaskQueue { get; } = new();

    private ProcessorVisitor<TValue> ProcessorVisitor { get; }

    private object LockObject { get; } = new();

    private Func<TValue?, Task>? OnDequeue { get; }

    private void OnTaskComplete(TaskQueueItem taskQueueItem)
    {
        if (taskQueueItem.IsRemoveFromParentWhenEmpty)
        {
            lock (LockObject)
            {
                if (taskQueueItem.TaskEmptySemaphore.Count == 0)
                {
                    TaskQueue.Remove(taskQueueItem.Key, out _);
                    taskQueueItem.Cancellation.Cancel();
                }
            }
        }
    }

    private TaskQueueItem GetTaskQueueItem(TKey key)
    {
        lock (LockObject)
        {
            if (!TaskQueue.TryGetValue(key, out var queue))
            {
                queue = new TaskQueueItem(key, ProcessorVisitor, OnTaskComplete);
                queue.TaskQueue.OnDequeue = async (item) =>
                {
                    if (queue.OnDequeue != null)
                    {
                        await queue.OnDequeue(item);
                    }
                    if (OnDequeue != null)
                    {
                        await OnDequeue(item);
                    }
                };
                TaskQueue[key] = queue;
                _ = queue.Start();
            }
            return queue;
        }
    }

    /// <summary>
    /// 向队列中添加值，并通知等待队列的处理者
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public TValue Enqueue(TKey key, TValue value)
    {
        var taskQueueItem = GetTaskQueueItem(key);
        taskQueueItem.TaskQueue.Enqueue(value);
        taskQueueItem.TaskEmptySemaphore.Increment();
        return value;
    }

    /// <summary>
    /// 从队列中取出值，如果队列为空，则等待
    /// </summary>
    /// <returns></returns>
    public async Task<TValue> Dequeue(TKey key)
    {
        return await GetTaskQueueItem(key).TaskQueue.Dequeue();
    }

    /// <summary>
    /// 设置当任务被取出时触发
    /// </summary>
    /// <param name="key"></param>
    /// <param name="onTaskDequeue"></param>
    public void SetOnTaskDequeue(TKey key, Func<TValue?, Task>? onTaskDequeue)
    {
        GetTaskQueueItem(key).OnDequeue = onTaskDequeue;
    }

    /// <summary>
    /// 设置并发数量
    /// </summary>
    /// <param name="key"></param>
    /// <param name="concurrent"></param>
    public void SetConcurrent(TKey key, int concurrent)
    {
        GetTaskQueueItem(key).SetConcurrent(concurrent);
    }

    /// <summary>
    /// 并行的处理者数量
    /// </summary>
    public int ProcessorCount { get; private set; } = 0;

    /// <summary>
    /// 当前空闲处理者数量
    /// </summary>
    public int IdleProcessorCount => ProcessorVisitor.CurrentCount;

    /// <summary>
    /// 取消等待队列
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isReleaseResourcesWhenCancel"></param>
    public void Cancel(TKey key, bool isReleaseResourcesWhenCancel = true)
    {
        var taskQueueItem = GetTaskQueueItem(key);
        if (taskQueueItem.Cancellation.IsCancellationRequested) return;
        taskQueueItem.IsReleaseResourcesWhenCancel = isReleaseResourcesWhenCancel;
        taskQueueItem.Cancellation.Cancel();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private async Task ReleaseResources()
    {
        List<Task> tasks = new();
        foreach (var taskQueueItem in TaskQueue.Values)
        {
            tasks.Add(taskQueueItem.ReleaseResources());
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 等待任务队列为空，并且所有处理者都完成
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task WaitForTaskEmptyAsync(TKey key)
    {
        await GetTaskQueueItem(key).TaskEmptySemaphore.WaitUntilZeroAsync();
    }

    /// <summary>
    /// 等待任务队列为空，并且所有处理者都完成
    /// </summary>
    /// <returns></returns>
    public async Task WaitForTaskEmptyAsync(TKey key, TimeSpan timeout)
    {
        await GetTaskQueueItem(key).TaskEmptySemaphore.WaitUntilZeroAsync().WaitAsync(timeout);
    }

}

internal class ProcessorVisitor<TValue>(int maxCount, IProcessor<TValue> processor)
{
    private SemaphoreSlim Semaphore { get; } = new(maxCount);

    private IProcessor<TValue> Processor { get; } = processor;

    public async Task<IProcessor<TValue>> Dequeue()
    {
        await Semaphore.WaitAsync();
        return Processor;
    }

    public void Enqueue()
    {
        Semaphore.Release();
    }

    public int CurrentCount => Semaphore.CurrentCount;

}