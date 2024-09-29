namespace TidyHPC.Locks;

/// <summary>
/// 锁定列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class LockList<T> : LockIList<List<T>, T>,ICollection<T>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockList(List<T> value) : base(value)
    {
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="values"></param>
    public void AddRange(IEnumerable<T> values)
    {
        lock (locker) Value.AddRange(values);
    }

    /// <summary>
    /// 拷贝后再遍历
    /// </summary>
    /// <param name="forEach"></param>
    /// <returns></returns>
    public async Task ForeachCopyAsync(Func<T, Task> forEach)
    {
        List<T> copy;
        lock (locker)
        {
            copy = new List<T>(Value);
        }
        foreach (var i in copy)
        {
            await forEach(i);
        }
        copy.Clear();
    }

}
