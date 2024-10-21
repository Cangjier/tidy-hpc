using System.Collections.Concurrent;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Layouts;
using TidyHPC.LiteDB.Hashes;
using TidyHPC.LiteDB.Metas;
using TidyHPC.Locks;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Caches;
internal class Cache
{
    public Cache(Database database)
    {
        Database = database;
        DictionaryVisitor = new DictionaryVisitor(this);
    }

    public Database Database { get; }

    /// <summary>
    /// <para>InterfaceNames的缓存，可以参考</para>
    /// <para>1. 当注册时会加入该集合</para>
    /// <para>2. 当GetInterfaceNames时会加入该集合</para>
    /// </summary>
    internal Lock<HashSet<string>> CacheInterfaceNames { get; } = new(new());

    internal async Task<byte[]> DequeueBuffer(int size)
    {
        int unitSize = 64;
        int formatSize = unitSize;
        int scale = 2;
        while (formatSize < size)
        {
            formatSize = unitSize * scale;
            scale *= 2;
        }
        if (formatSize > 128)
        {
            return await BufferDictionary.GetOrAdd(formatSize, _ => InitialBuffer(new(), formatSize, Database.FileStream.StreamCount * 2)).Dequeue();
        }
        else
        {
            return await BufferDictionary.GetOrAdd(formatSize, _ => InitialBuffer(new(), formatSize, Database.FileStream.StreamCount * 8)).Dequeue();
        }

    }

    internal void EnqueueBuffer(byte[] buffer)
    {
        BufferDictionary[buffer.Length].Enqueue(buffer);
    }

    private WaitQueue<byte[]> InitialBuffer(WaitQueue<byte[]> bufferQueue, int size, int count)
    {
        for (int i = 0; i < count; i++)
        {
            bufferQueue.Enqueue(new byte[size]);
        }
        return bufferQueue;
    }

    private ConcurrentDictionary<long, WaitQueue<byte[]>> BufferDictionary { get; } = new();

    /// <summary>
    /// IO缓存
    /// </summary>
    internal ByteArrayCache IOCache { get; } = new();

    internal WaitQueue<StatisticalLayout> StatisticalBlockPool { get; } = new WaitQueue<StatisticalLayout>().Initialize(64, () => new StatisticalLayout());

    internal WaitQueue<HashTable<Int64Value>> HashTablePool { get; } = new WaitQueue<HashTable<Int64Value>>().Initialize(64, () => new HashTable<Int64Value>());

    internal WaitQueue<HashBlock<Int64Value>> HashBlockPool { get; } = new WaitQueue<HashBlock<Int64Value>>().Initialize(64, () => new HashBlock<Int64Value>());

    internal WaitQueue<MetaBlock> MetaBlockPool { get; } = new WaitQueue<MetaBlock>().Initialize(64, () => new MetaBlock());

    internal DictionaryVisitor DictionaryVisitor { get; }
}
