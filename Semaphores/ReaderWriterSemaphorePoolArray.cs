namespace TidyHPC.Semaphores;

/// <summary>
/// 读写信号量池集合
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class ReaderWriterSemaphorePoolArray<TKey>
    where TKey : notnull
{
    /// <summary>
    /// 初始化信号量池集合
    /// </summary>
    /// <param name="count"></param>
    /// <param name="readerCount"></param>
    public ReaderWriterSemaphorePoolArray(int readerCount,int count)
    {
        Pools = new ReaderWriterSemaphorePool<TKey>[count];
        for (int i = 0; i < count; i++)
        {
            Pools[i] = new ReaderWriterSemaphorePool<TKey>(readerCount);
        }
    }

    private ReaderWriterSemaphorePool<TKey>[] Pools { get; }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ReaderWriterSemaphorePool<TKey> this[int index] => Pools[index];
}