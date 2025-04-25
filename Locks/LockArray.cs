using System.Collections;

namespace TidyHPC.Locks;

/// <summary>
/// 锁定数组
/// </summary>
/// <typeparam name="T"></typeparam>
public class LockArray<T> : Lock<T[]>, IEnumerable
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockArray(T[] value) : base(value)
    {
    }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index]
    {
        get => Process(array => array[index]);
        set => Process(array => array[index] = value);
    }

    /// <summary>
    /// 长度
    /// </summary>
    public int Length => Process(array => array.Length);

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetEnumerator() => Process(array => array.GetEnumerator());

    /// <summary>
    /// 克隆
    /// </summary>
    /// <returns></returns>
    public T[] Clone() => Process(array => (T[])array.Clone());
}
