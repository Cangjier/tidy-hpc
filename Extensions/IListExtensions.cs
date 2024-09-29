namespace TidyHPC.Extensions;

/// <summary>
/// List extensions
/// </summary>
public static class IListExtensions
{
    /// <summary>
    /// 替换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public static void Replace<T>(this IList<T> self,int index,T value)
    {
        self.RemoveAt(index);
        self.Insert(index, value);
    }

    /// <summary>
    /// 执行任务队列
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static async Task Invoke(this IList<Func<Task>> self)
    {
        await self.ForeachAsync(async i =>
        {
            await i.Invoke();
        });
    }
}
