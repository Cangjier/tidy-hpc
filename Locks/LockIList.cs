namespace TidyHPC.Locks;

/// <summary>
/// 锁定列表
/// </summary>
/// <typeparam name="TList"></typeparam>
/// <typeparam name="T"></typeparam>
public class LockIList<TList, T> : LockEnumerable<TList, T>,IList<T>
    where TList : IList<T>

{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockIList(TList value) : base(value)
    {
    }

    /// <summary>
    /// 索引
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index]
    {
        get
        {
            lock (locker)
            {
                return Value[index];
            }
        }
        set
        {
            lock (locker)
            {
                Value[index] = value;
            }
        }
    }

    /// <summary>
    /// 获取数量
    /// </summary>
    public int Count
    {
        get
        {
            lock(locker)return Value.Count;
        }
    }

    /// <summary>
    /// 是否只读
    /// </summary>
    public bool IsReadOnly
    {
        get
        {
            lock (locker) return Value.IsReadOnly;
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        lock(locker)Value.Add(item);
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        lock(locker)Value.Clear();
    }

    /// <summary>
    /// 是否包含
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        lock(locker)return Value.Contains(item);
    }

    /// <summary>
    /// 拷贝至
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock(locker)Value.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// 索引
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(T item)
    {
        lock(locker)return Value.IndexOf(item);
    }

    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, T item)
    {
        lock(locker)Value.Insert(index, item);
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item)
    {
        lock(locker)return Value.Remove(item);
    }

    /// <summary>
    /// 移除指定项
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void RemoveAt(int index)
    {
        lock (locker) Value.RemoveAt(index);
    }

    /// <summary>
    /// 替换
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public void Replace(int index,T value)
    {
        lock (locker)
        {
            Value.RemoveAt(index);
            Value.Insert(index, value);
        }
    }
}
