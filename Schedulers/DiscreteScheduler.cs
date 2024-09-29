using System.Collections.Concurrent;
using TidyHPC.Locks;

namespace TidyHPC.Schedulers;

/// <summary>
/// 离散计划器
/// </summary>
public class DiscreteScheduler
{
    /// <summary>
    /// 计划任务
    /// </summary>
    private class ScheduledTask : IComparable<ScheduledTask>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="executeAt"></param>
        public ScheduledTask(DateTime executeAt)
        {
            ExecuteAt = executeAt;
            Tasks = new();
        }

        /// <summary>
        /// 计划执行时间点
        /// </summary>
        public DateTime ExecuteAt { get; }

        /// <summary>
        /// 待执行任务
        /// </summary>
        public ConcurrentDictionary<Guid, Func<Task>> Tasks { get; private set; }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ScheduledTask? other)
        {
            if (other == null)
            {
                return 1;
            }
            return ExecuteAt.CompareTo(other.ExecuteAt);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public Guid AddTask(Func<Task> task)
        {
            Guid id = Guid.NewGuid();
            Tasks.TryAdd(id, task);
            return id;
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTask(Guid id)
        {
            Tasks.TryRemove(id, out _);
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Run()
        {
            foreach (var pair in Tasks)
            {
                Task.Run(pair.Value);
            }
        }

        /// <summary>
        /// 清空自身
        /// </summary>
        public void Release()
        {
            if (Tasks != null)
            {
                Tasks.Clear();
                Tasks = null!;
            }
        }
    }

    /// <summary>
    /// 计划索引
    /// </summary>
    public struct Index
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime Key;

        /// <summary>
        /// 任务的唯一标识符
        /// </summary>
        public Guid ID;

        /// <summary>
        /// 索引是否有效
        /// </summary>
        public readonly bool IsValid => Key != default && ID != default;
    }

    /// <summary>
    /// 离散计划器
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="start"></param>
    public DiscreteScheduler(int interval = 500, bool start = false)
    {
        this.Interval = interval;
        if (start)
        {
            _ = Task.Run(StartAsync);
        }
    }

    private readonly LockSortedDictionary<DateTime, ScheduledTask> Master = new(new());
    private readonly CancellationTokenSource Cancelable = new();
    private readonly int Interval = 500;

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="task"></param>
    /// <returns></returns>
    public Index AddTask(TimeSpan delay, Func<Task> task)
    {
        var key = DateTime.UtcNow + delay;
        var msTicks = key.Ticks % TimeSpan.TicksPerSecond;
        var point = msTicks / TimeSpan.TicksPerMillisecond / Interval * Interval + Interval;
        key = key.AddTicks(-msTicks).AddMilliseconds(point);
        Index result = new()
        {
            Key = key
        };
        Master.AddOrUpdate(key,
            () =>
            {
                var schedularTask = new ScheduledTask(key);
                var id = schedularTask.AddTask(task);
                result.ID = id;
                return schedularTask;
            },
            old =>
            {
                var id = old.AddTask(task);
                result.ID = id;
                return old;
            });
        lock (Cancelable)
        {
            Monitor.Pulse(Cancelable);
        }
        return result;
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="index"></param>
    public void RemoveTask(Index index)
    {
        Master.Update(index.Key, res =>
        {
            if (res != null)
            {
                res.RemoveTask(index.ID);
            }
            return res;
        });
    }

    /// <summary>
    /// 启动
    /// </summary>
    /// <returns></returns>
    public async Task StartAsync()
    {
        while (!Cancelable.Token.IsCancellationRequested)
        {
            ScheduledTask? nextTask;
            while (!Master.TryPeek(out nextTask))
            {
                await Task.Delay(Interval);
            }
            var delay = nextTask!.ExecuteAt - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                lock (Cancelable)
                {
                    Monitor.Wait(Cancelable, delay);
                }
            }
            else
            {
                if (Master.TryDequeue(out nextTask))
                {
                    try
                    {
                        nextTask?.Run();
                    }
                    catch
                    {

                    }
                    nextTask?.Release();
                }
            }
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        Cancelable.Cancel();
    }
}
