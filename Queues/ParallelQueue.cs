using TidyHPC.Semaphores;

namespace TidyHPC.Queues;

/// <summary>
/// 并行数量
/// </summary>
public class ParallelQueueOptions
{
    /// <summary>
    /// 最大并行数量
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
}

/// <summary>
/// 并行控制器
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class ParallelController<TResult>(Action<TResult?> _return)
{
    /// <summary>
    /// 设置结果
    /// </summary>
    private readonly Action<TResult?> _Return = _return;

    /// <summary>
    /// 设置结果
    /// </summary>
    /// <param name="result"></param>
    public void Return(TResult result)
    {
        _Return.Invoke(result);
    }

    /// <summary>
    /// 设置结果
    /// </summary>
    public void Return()
    {
        _Return.Invoke(default);
    }
}

/// <summary>
/// 并行
/// </summary>
public class ParallelQueue
{
    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="options"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<TResult?> For<TResult>(int start, int end, ParallelQueueOptions options, Func<int, ParallelController<TResult>, Task> func)
    {

        TaskCompletionSource<TResult?> onComplete = new();
        SemaphoreWrap<int> semaphoreProducer = new(start);
        SemaphoreWrap<int> semaphoreConsumer = new(0);
        int count = end - start;
        TaskProcessorQueue<int> queue = new();
        ParallelController<TResult> parallelController = new((result) =>
        {
            queue.Cancel();
            onComplete.TrySetResult(result);
        });
        for (; semaphoreProducer.Value < end && semaphoreProducer.Value < start + options.MaxDegreeOfParallelism; semaphoreProducer.Value++)
        {
            queue.Enqueue(semaphoreProducer.Value);
        }
        queue.AddProcessor(async (value) =>
        {
            //生产者逻辑
            _ = Task.Run(async () =>
            {
                await semaphoreProducer.Process(() =>
                {
                    if (semaphoreProducer.Value < end)
                    {
                        queue.Enqueue(semaphoreProducer.Value);
                        semaphoreProducer.Value++;
                    }
                });
            });
            await func(value, parallelController);
            //消费者逻辑
            _ = Task.Run(async () =>
            {
                await semaphoreConsumer.Process(() =>
                {
                    semaphoreConsumer.Value++;
                    if (semaphoreConsumer.Value == count)
                    {
                        onComplete.TrySetResult(default);
                    }
                });
            });
        }, options.MaxDegreeOfParallelism);
        _ = Task.Run(queue.Start);
        var result = await onComplete.Task;
        queue.Cancel();
        return result;
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="options"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, ParallelQueueOptions options, Action<int, ParallelController<object>> func)
    {
        await For<object>(start, end, options, async (value, controller) =>
        {
            func(value, controller);
            await Task.CompletedTask;
        });
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="options"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, ParallelQueueOptions options, Func<int, ParallelController<object>, Task> func)
    {
        await For<object>(start, end, options, async (value, controller) =>
        {
            await func(value, controller);
        });
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="options"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, ParallelQueueOptions options, Action<int> func)
    {
        await For<object>(start, end, options, async (value, controller) =>
        {
            func(value);
            await Task.CompletedTask;
        });
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="options"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, ParallelQueueOptions options, Func<int,Task> func)
    {
        await For<object>(start, end, options, async (value, controller) =>
        {
            await func(value);
        });
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, Func<int, Task> func)
    {
        await For(start, end, new ParallelQueueOptions(), async (value, controller) =>
        {
            await func(value);
        });
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, Action<int> func)
    {
        await For(start, end, new ParallelQueueOptions(), func);
    }

    /// <summary>
    /// 并行遍历
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task For(int start, int end, Func<int, ParallelController<object>, Task> func)
    {
        await For(start, end, new ParallelQueueOptions(), func);
    }
}
