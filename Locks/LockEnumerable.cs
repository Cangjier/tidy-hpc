using System.Collections;

namespace TidyHPC.Locks;

/// <summary>
/// 锁定可枚举值
/// </summary>
/// <typeparam name="TEnumerable"></typeparam>
/// <typeparam name="T"></typeparam>
public class LockEnumerable<TEnumerable, T> : Lock<TEnumerable>,IEnumerable<T>
    where TEnumerable : IEnumerable<T>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockEnumerable(TEnumerable value) : base(value)
    {
    }

    /// <summary>
    /// 遍历
    /// </summary>
    /// <param name="onProcess"></param>
    public void Foreach(Action<T> onProcess)
    {
        lock (locker)
        {
            foreach(var item in Value)
            {
                onProcess(item);
            }
        }
    }

    /// <summary>
    /// 遍历
    /// </summary>
    /// <param name="onProcess"></param>
    public void Foreach(Action<T,int> onProcess)
    {
        lock (locker)
        {
            int index = 0;
            foreach (var item in Value)
            {
                onProcess(item, index++);
            }
        }
    }

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        foreach(var item in Value)
        {
            yield return item;
        }
    }

    /// <summary>
    /// 获取枚举器
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in Value)
        {
            yield return item;
        }
    }
}
