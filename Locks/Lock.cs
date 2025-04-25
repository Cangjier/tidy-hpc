namespace TidyHPC.Locks;

/// <summary>
/// 锁定值
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Lock<TValue>(TValue value)
{
    /// <summary>
    /// Implicit convert Lock to TValue
    /// </summary>
    /// <param name="lockValue"></param>
    public static implicit operator TValue(Lock<TValue> lockValue) => lockValue.Value;

    /// <summary>
    /// Implicit convert TValue to Lock
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Lock<TValue>(TValue value) => new(value);

    /// <summary>
    /// 锁定的目标值
    /// </summary>
    public TValue Value { get; set; }= value;

    /// <summary>
    /// 锁对象
    /// </summary>
    protected object locker { get; } = new();

    /// <summary>
    /// 锁定并处理值
    /// </summary>
    /// <param name="onProcess"></param>
    public void Process(Action onProcess)
    {
        lock(locker)
        {
            onProcess();
        }
    }

    /// <summary>
    /// 锁定并处理值
    /// </summary>
    /// <param name="onProcess"></param>
    public void Process(Action<TValue> onProcess)
    {
        lock (locker)
        {
            onProcess(Value);
        }
    }

    /// <summary>
    /// 锁定并处理值
    /// </summary>
    /// <param name="onProcess"></param>
    /// <returns></returns>
    public TResult Process<TResult>(Func<TValue, TResult> onProcess)
    {
        lock (locker)
        {
            return onProcess(Value);
        }
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Lock<TValue> Set(TValue value)
    {
        lock (locker) Value = value;
        return this;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="onSet"></param>
    /// <returns></returns>
    public Lock<TValue> Set(TValue value, Action onSet)
    {
        lock (locker)
        {
            Value = value;
            onSet();
        }

        return this;
    }

    /// <summary>
    /// 取值
    /// </summary>
    /// <param name="onGet"></param>
    /// <returns></returns>
    public Lock<TValue> Get(Action<TValue> onGet)
    {
        lock (locker)
        {
            onGet(Value);
        }
        return this;
    }

    /// <summary>
    /// 更新值
    /// </summary>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    public Lock<TValue> Update(Func<TValue, TValue> onUpdate)
    {
        lock (locker)
        {
            Value = onUpdate(Value);
        }
        return this;
    }

    /// <summary>
    /// 更新值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public Lock<TValue> Update(TValue value)
    {
        lock (locker)
        {
            Value = value;
        }
        return this;
    }   

    /// <summary>
    /// 更新值
    /// </summary>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    public Lock<TValue> Update(Func<TValue> onUpdate)
    {
        lock (locker)
        {
            Value = onUpdate();
        }
        return this;
    }
}
