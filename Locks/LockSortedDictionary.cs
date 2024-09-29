using System.Diagnostics.CodeAnalysis;

namespace TidyHPC.Locks;

/// <summary>
/// 锁定的排序字典
/// </summary>
public class LockSortedDictionary<TKey, TValue> : Lock<SortedDictionary<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockSortedDictionary(SortedDictionary<TKey, TValue> value) : base(value)
    {
    }

    /// <summary>
    /// 添加或更新
    /// </summary>
    /// <param name="key"></param>
    /// <param name="onAdd"></param>
    /// <param name="onUpdate"></param>
    public void AddOrUpdate(TKey key, Func<TValue> onAdd, Func<TValue, TValue> onUpdate)
    {
        lock (locker)
        {
            if (Value.ContainsKey(key))
            {
                TValue value = onUpdate(Value[key]);
                Value[key] = value;
            }
            else
            {
                TValue value = onAdd();
                Value.Add(key, value);
            }
        }
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="key"></param>
    /// <param name="update"></param>
    public void Update(TKey key, Func<TValue?, TValue?> update)
    {
        TValue? value;
        lock (locker)
        {
            if (Value.ContainsKey(key))
            {
                value = update(Value[key]);
                if (value != null)
                {
                    Value[key] = value;
                }
            }
            else
            {
                value = update(default);
                if (value != null)
                {
                    Value.Add(key, value);
                }
            }
        }
    }

    /// <summary>
    /// 尝试获取
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryPeek([MaybeNullWhen(false)]out TValue result)
    {
        lock (locker)
        {
            if (Value.Count == 0)
            {
                result = default;
                return false;
            }
            var first = Value.First();
            result = first.Value;
            return true;
        }
    }

    /// <summary>
    /// 尝试取出
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryDequeue([MaybeNullWhen(false)] out TValue result)
    {
        lock (locker)
        {
            if (Value.Count == 0)
            {
                result = default;
                return false;
            }
            var first = Value.First();
            result = first.Value;
            Value.Remove(first.Key);
            return true;
        }
    }
}
