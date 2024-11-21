using System.Collections.Concurrent;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 字节缓存
/// </summary>
public class BytesCache
{
    /// <summary>
    /// 字节缓存构造函数
    /// </summary>
    /// <param name="config"></param>
    public BytesCache(BytesCacheConfig config)
    {
        Config = config;
    }

    /// <summary>
    /// 字节缓存配置
    /// </summary>
    public BytesCacheConfig Config { get; }

    private ConcurrentDictionary<long, WaitQueue<byte[]>> BufferDictionary { get; } = new();

    private ConcurrentDictionary<int, int> CacheSize { get; } = new();

    /// <summary>
    /// 获取Buffer
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public async Task<byte[]> DequeueBuffer(int size)
    {
        int unitSize = 64;
        int formatSize = unitSize;
        if (CacheSize.TryGetValue(size, out formatSize) == false)
        {
            int scale = 2;
            while (formatSize < size)
            {
                formatSize = unitSize * scale;
                scale *= 2;
            }
            CacheSize.TryAdd(size, formatSize);
        }
        
        if (formatSize > 128)
        {
            return await BufferDictionary.GetOrAdd(formatSize, _ => InitialBuffer(new(), formatSize, Config.BigBytesCacheCount)).Dequeue();
        }
        else
        {
            return await BufferDictionary.GetOrAdd(formatSize, _ => InitialBuffer(new(), formatSize, Config.SmallBytesCacheCount)).Dequeue();
        }

    }

    /// <summary>
    /// 归还Buffer
    /// </summary>
    /// <param name="buffer"></param>
    public void EnqueueBuffer(byte[] buffer)
    {
        BufferDictionary[buffer.Length].Enqueue(buffer);
    }

    /// <summary>
    /// 使用Buffer
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public async Task<UsingBytes> Using(int size)
    {
        return new(this, await DequeueBuffer(size));
    }

    private WaitQueue<byte[]> InitialBuffer(WaitQueue<byte[]> bufferQueue, int size, int count)
    {
        for (int i = 0; i < count; i++)
        {
            bufferQueue.Enqueue(new byte[size]);
        }
        return bufferQueue;
    }
}
