namespace TidyHPC.Semaphores;

/// <summary>
/// 信号量封装
/// </summary>
public class SemaphoreWrap<T>
where T : notnull
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    /// <param name="initialCount"></param>
    /// <param name="maxCount"></param>
    public SemaphoreWrap(T value, int initialCount, int maxCount)
    {
        Value = value;
        Semaphore = new(initialCount, maxCount);
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public SemaphoreWrap(T value)
    {
        Value = value;
        Semaphore = new(1, 1);
    }

    private readonly SemaphoreSlim Semaphore;

    /// <summary>
    /// 信号量对应的值
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// 信号量增加
    /// </summary>
    /// <returns></returns>
    public async Task WaitAsync()
    {
        await Semaphore.WaitAsync();
    }

    /// <summary>
    /// 信号量减少
    /// </summary>
    /// <returns></returns>
    public void Release()
    {
        Semaphore.Release();
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="onProcess"></param>
    /// <returns></returns>
    public async Task Process(Action onProcess)
    {
        await WaitAsync();
        try
        {
            onProcess();
        }
        finally
        {
            Release();
        }
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="onProcess"></param>
    /// <returns></returns>
    public async Task Process(Action<T> onProcess)
    {
        await WaitAsync();
        try
        {
            onProcess(Value);
        }
        finally
        {
            Release();
        }
    }

    /// <summary>
    /// 处理
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="onProcess"></param>
    /// <returns></returns>
    public async Task<TResult> Process<TResult>(Func<TResult> onProcess)
    {
        await WaitAsync();
        try
        {
            return onProcess();
        }
        finally
        {
            Release();
        }
    }
}
