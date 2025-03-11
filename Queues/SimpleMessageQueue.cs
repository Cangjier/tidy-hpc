using System.Collections.Concurrent;

namespace TidyHPC.Queues;

/// <summary>
/// 简易消息队列
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class SimpleMessageQueue<TValue>:IDisposable
{
    private ConcurrentQueue<TValue> Target { get; set; } = new();

    /// <summary>
    /// 添加值到队列中
    /// </summary>
    /// <param name="value"></param>
    public void Enqueue(TValue value)
    {
        Target.Enqueue(value);
    }

    /// <summary>
    /// 消息处理者
    /// </summary>
    public Func<TValue,Task<bool>>? Processer { get; set; }

    private bool _isRunning = false;

    /// <summary>
    /// 通知等待队列的处理者
    /// </summary>
    /// <param name="retryCount">尝试次数</param>
    /// <returns></returns>
    public async Task Notify(int retryCount=1)
    {
        if (_isRunning)
        {
            return;
        }
        _isRunning = true;
        if (Processer is not null)
        {
            bool containsError = false;
            for (var i = 0; i < retryCount && (containsError || i == 0); i++)
            {
                containsError = false;
                while (Target.TryPeek(out var value))
                {
                    if (await Processer(value))
                    {
                        Target.TryDequeue(out _);
                    }
                    else
                    {
                        containsError = true;
                        break;
                    }
                }
            }
        }
        _isRunning = false;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Target?.Clear();
        Target = null!;
        Processer = null!;
    }
}
