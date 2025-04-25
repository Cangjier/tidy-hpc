using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TidyHPC.Locks;

/// <summary>
/// 锁定字典
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class LockDictionary<TKey, TValue> : Lock<Dictionary<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockDictionary(Dictionary<TKey, TValue> value) : base(value)
    {
    }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue this[TKey key]
    {
        get => Process(dict => dict[key]);
        set => Process(dict => dict[key] = value);
    }

    /// <summary>
    /// 数量
    /// </summary>
    public int Count => Process(dict => dict.Count);

    /// <summary>
    /// 键集合
    /// </summary>
    public Dictionary<TKey, TValue>.KeyCollection Keys => Process(dict => dict.Keys);

    /// <summary>
    /// 值集合
    /// </summary>
    public Dictionary<TKey, TValue>.ValueCollection Values => Process(dict => dict.Values);

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(TKey key, TValue value) => Process(dict => dict.Add(key, value));

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear() => Process(dict => dict.Clear());

    /// <summary>
    /// 是否包含键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(TKey key) => Process(dict => dict.ContainsKey(key));

    /// <summary>
    /// 是否包含值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool ContainsValue(TValue value) => Process(dict => dict.ContainsValue(value));

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Process(dict => dict.GetEnumerator());

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(TKey key) => Process(dict => dict.Remove(key));

    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        TValue? tempValue = default;
        var result = Process(dict =>
        {
            return dict.TryGetValue(key, out var tempValue);
        });
        if (result)
        {
            value = tempValue;
        }
        else
        {
            value = default;
        }

        return result;
    }

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

