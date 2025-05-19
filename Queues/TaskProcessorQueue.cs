using TidyHPC.Semaphores;

namespace TidyHPC.Queues;

/// <summary>
/// 处理者
/// </summary>
public interface IProcessor<TValue>
{
    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task Process(TValue value);
}

/// <summary>
/// 简易的处理者
/// </summary>
/// <typeparam name="TValue"></typeparam>
public struct SimpleProcessor<TValue> : IProcessor<TValue>
{
    private Func<TValue, Task> OnProcess { get; }

    /// <summary>
    /// 简易处理者的构造函数
    /// </summary>
    /// <param name="onProcess"></param>
    public SimpleProcessor(Func<TValue, Task> onProcess)
    {
        OnProcess = onProcess;
    }

    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task Process(TValue value)
    {
        return OnProcess(value);
    }
}

/// <summary>
/// 等待队列
/// </summary>
/// <typeparam name="TValue">队列值类型</typeparam>
public class TaskProcessorQueue<TValue>
{
    /// <summary>
    /// 等待队列
    /// </summary>
    public TaskProcessorQueue()
    {

    }

    private WaitQueue<TValue> TaskQueue { get; } = new();

    private WaitQueue<IProcessor<TValue>> ProcessorQueue { get; } = new();

    private ReverseSemaphore TaskEmptySemaphore { get; } = new();

    /// <summary>
    /// 取消等待队列
    /// </summary>
    private CancellationTokenSource Cancellation { get; } = new();

    /// <summary>
    /// 向队列中添加值，并通知等待队列的处理者
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public TValue Enqueue(TValue value)
    {
        TaskQueue.Enqueue(value);
        TaskEmptySemaphore.Increment();
        return value;
    }

    /// <summary>
    /// 从队列中取出值，如果队列为空，则等待
    /// </summary>
    /// <returns></returns>
    public async Task<TValue> Dequeue()
    {
        return await TaskQueue.Dequeue();
    }

    /// <summary>
    /// 当任务被取出时触发
    /// </summary>
    public Func<TValue?, Task>? OnTaskDequeue
    {
        get => TaskQueue.OnDequeue;
        set => TaskQueue.OnDequeue = value;
    }

    private bool IsReleaseResourcesWhenCancel { get; set; } = true;

    /// <summary>
    /// 并行的处理者数量
    /// </summary>
    public int ProcessorCount { get; private set; } = 0;

    /// <summary>
    /// 当前等待队列的任务数量
    /// </summary>
    public int BlockingTaskCount => TaskQueue.CurrentCount;

    /// <summary>
    /// 当前空闲处理者数量
    /// </summary>
    public int IdleProcessorCount => ProcessorQueue.CurrentCount;

    /// <summary>
    /// 取消等待队列
    /// </summary>
    /// <param name="isReleaseResourcesWhenCancel"></param>
    public void Cancel(bool isReleaseResourcesWhenCancel = true)
    {
        if (Cancellation.IsCancellationRequested) return;
        IsReleaseResourcesWhenCancel = isReleaseResourcesWhenCancel;
        Cancellation.Cancel();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private async Task ReleaseResources()
    {
        Cancellation.Dispose();
        await TaskQueue.ReleaseResources();
        await ProcessorQueue.ReleaseResources();
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
            var task = await Dequeue();
            if (Cancellation.IsCancellationRequested)
            {
                if (IsReleaseResourcesWhenCancel)
                {
                    await ReleaseResources();
                }
                break;
            }
            var processor = await ProcessorQueue.Dequeue();
            async Task func()
            {
                await processor.Process(task);
                TaskEmptySemaphore.Decrement();
                ProcessorQueue.Enqueue(processor);
            }
            _ = func();
        }
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="onProcess"></param>
    /// <param name="processCount"></param>
    /// <returns></returns>
    public async Task Start(Func<TValue, Task> onProcess, int processCount)
    {
        for (int i = 0; i < processCount; i++)
        {
            AddProcessor(new SimpleProcessor<TValue>(onProcess));
        }
        await Start();
    }

    /// <summary>
    /// 等待任务队列为空，并且所有处理者都完成
    /// </summary>
    /// <returns></returns>
    public async Task WaitForTaskEmptyAsync()
    {
        await TaskEmptySemaphore.WaitUntilZeroAsync();
    }

    /// <summary>
    /// 添加处理者
    /// </summary>
    public void AddProcessor(IProcessor<TValue> processor)
    {
        ProcessorCount += 1;
        ProcessorQueue.Enqueue(processor);
    }

    /// <summary>
    /// 添加多个处理者
    /// </summary>
    /// <param name="processor"></param>
    /// <param name="count"></param>
    public void AddProcessor(Func<IProcessor<TValue>> processor, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddProcessor(processor());
        }
    }

    /// <summary>
    /// 添加多个处理者
    /// </summary>
    /// <param name="processors"></param>
    public void AddProcessor(IEnumerable<IProcessor<TValue>> processors)
    {
        foreach (var processor in processors)
        {
            AddProcessor(processor);
        }
    }

    /// <summary>
    /// 添加处理者
    /// </summary>
    /// <param name="onProcess"></param>
    public void AddProcessor(Func<TValue, Task> onProcess)
    {
        AddProcessor(new SimpleProcessor<TValue>(onProcess));
    }

    /// <summary>
    /// 添加多个处理者
    /// </summary>
    /// <param name="onProcess"></param>
    /// <param name="count"></param>
    public void AddProcessor(Func<TValue, Task> onProcess, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddProcessor(onProcess);
        }
    }

    /// <summary>
    /// 添加处理者
    /// </summary>
    /// <param name="onProcess"></param>
    public void AddProcessor(Action<TValue> onProcess)
    {
        AddProcessor(new SimpleProcessor<TValue>(value =>
        {
            onProcess(value);
            return Task.CompletedTask;
        }));
    }

    /// <summary>
    /// 添加多个处理者
    /// </summary>
    /// <param name="onProcess"></param>
    /// <param name="count"></param>
    public void AddProcessor(Action<TValue> onProcess, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddProcessor(onProcess);
        }
    }

    /// <summary>
    /// 移除处理者
    /// </summary>
    public async Task<IProcessor<TValue>> RemoveProcessor()
    {
        ProcessorCount -= 1;
        var result = await ProcessorQueue.Dequeue();
        if (result == null)
        {
            throw new Exception("processor is null");
        }
        return result;
    }

    /// <summary>
    /// 移除多个处理者
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IProcessor<TValue>>> RemoveProcessor(int count)
    {
        var result = new List<IProcessor<TValue>>();
        for (int i = 0; i < count; i++)
        {
            result.Add(await RemoveProcessor());
        }
        return result;
    }
}