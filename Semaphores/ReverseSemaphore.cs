namespace TidyHPC.Semaphores;

/// <summary>
/// 反向信号量
/// </summary>
public class ReverseSemaphore
{
    private int _count = 0;
    private readonly object _lock = new object();
    private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// 增加信号量
    /// </summary>
    public void Increment()
    {
        lock (_lock)
        {
            _count++;
            // 如果变为非0，就重置等待
            if (_count == 1)
            {
                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
    }

    /// <summary>
    /// 减少信号量
    /// </summary>
    public void Decrement()
    {
        lock (_lock)
        {
            if (_count > 0)
            {
                _count--;
                if (_count == 0)
                {
                    _tcs.TrySetResult(true); // 所有等待线程释放
                }
            }
        }
    }

    /// <summary>
    /// 等待信号量为0
    /// </summary>
    /// <returns></returns>
    public Task WaitUntilZeroAsync()
    {
        lock (_lock)
        {
            return _tcs.Task;
        }
    }
}
