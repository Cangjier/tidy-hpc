namespace TidyHPC.Locks;

/// <summary>
/// 锁定哈希集合
/// </summary>
/// <typeparam name="T"></typeparam>
public class LockHashSet<T> : Lock<HashSet<T>>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockHashSet(HashSet<T> value) : base(value)
    {
    }

    /// <summary>
    /// 添加范围
    /// </summary>
    /// <param name="values"></param>
    public void AddRange(IEnumerable<T> values) => Process(set => set.UnionWith(values));

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="value"></param>
    public void Add(T value) => Process(set => set.Add(value));

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="value"></param>
    public void Remove(T value) => Process(set => set.Remove(value));

    /// <summary>
    /// 移除范围
    /// </summary>
    /// <param name="values"></param>
    public void RemoveRange(IEnumerable<T> values) => Process(set => set.RemoveWhere(values.Contains));

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear() => Process(set => set.Clear());

    /// <summary>
    /// 包含
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(T value) => Process(set => set.Contains(value));

    /// <summary>
    /// 是否为空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty() => Process(set => set.Count == 0);


}
