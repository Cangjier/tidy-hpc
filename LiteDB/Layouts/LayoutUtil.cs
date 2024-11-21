using TidyHPC.Semaphores;

namespace TidyHPC.LiteDB.Layouts;

internal static class LayoutUtil
{
    /// <summary>
    /// 获取使用统计区域目标位是否被使用
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static bool IsUsed(byte[] buffer, int offset, int index)
    {
        // 计算字节索引和位索引  
        int byteIndex = index / 8;
        int bitIndex = index % 8;

        // 使用位操作检查指定位是否为1  
        return (buffer[offset + byteIndex] & 1 << bitIndex) != 0;
    }

    /// <summary>
    /// 取消使用统计区域目标位
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="index"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static void Unuse(byte[] buffer, int offset, int index)
    {
        // 计算字节索引和位索引  
        int byteIndex = index / 8;
        int bitIndex = index % 8;

        // 创建一个掩码，除了目标位之外所有位都为1  
        byte mask = (byte)~(1 << bitIndex);

        // 使用位操作清除指定位  
        buffer[offset + byteIndex] &= mask;
    }

    /// <summary>
    /// 使用统计区域目标位
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="index"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static void Use(byte[] buffer, int offset, int index)
    {
        // 计算字节索引和位索引  
        int byteIndex = index / 8;
        int bitIndex = index % 8;

        // 创建一个掩码，只有目标位为1  
        byte mask = (byte)(1 << bitIndex);

        // 使用位或操作设置指定位  
        buffer[offset + byteIndex] |= mask;
    }

    /// <summary>
    /// 获取第一个可用的位的索引
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    internal static int GetFirstUnused(byte[] buffer, int offset, int bitLength)
    {
        int byteIndex = 0;
        int bitIndex = 0;

        for (int i = 0; i < bitLength; i++)
        {
            // 使用位操作检查指定位是否为1  
            if ((buffer[offset + byteIndex] & 1 << bitIndex) == 0)
            {
                return i; // 返回第一个未使用的位的索引  
            }

            // 增加位索引，如果达到8，则重置为0并增加字节索引  
            bitIndex++;
            if (bitIndex == 8)
            {
                bitIndex = 0;
                byteIndex++;
            }
        }

        return -1; // 如果没有找到未使用的位，则返回-1  
    }

    /// <summary>
    /// 是否包含未使用的位
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    internal static bool ContainsUnused(byte[] buffer, int offset, int bitLength)
    {
        int fullByteLength = (int)Math.Floor(bitLength / 8.0); // 计算可以完整存储的字节数
        // 从字节数组的最后一个字节开始往前遍历
        for (int i = fullByteLength - 1; i >= 0; i--)
        {
            // 如果当前字节不为0xFF（即255，所有位都设置为1），那么存在未使用的位  
            if (buffer[i + offset] != 0xFF)
            {
                return true; // 找到一个未使用的位，立即返回true  
            }
        }
        // 如果存在未使用的位，那么它一定在最后一个字节中
        // 计算最后一个字节的位数
        int lastByteBitLength = bitLength % 8;
        // 遍历最后一个字节的位
        for (int i = 0; i < lastByteBitLength; i++)
        {
            // 如果当前位为0，那么存在未使用的位
            if ((buffer[fullByteLength + offset] & 1 << i) == 0)
            {
                return true; // 找到一个未使用的位，立即返回true  
            }
        }
        // 遍历完所有字节，没有发现未使用的位  
        return false;
    }

    /// <summary>
    /// 获取所有已使用的位的索引
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    internal static List<int> GetAllNoZeroIndex(byte[] buffer, int offset, int bitLength)
    {
        var result = new List<int>(capacity: bitLength); // 预分配容量
        var fullByteLength = (int)Math.Floor(bitLength / 8.0); // 计算可以完整存储的字节数
        for (int byteIndex = 0; byteIndex < fullByteLength; byteIndex++)
        {
            byte b = buffer[offset + byteIndex];
            if (b != 0) // 跳过值为0的字节，这是一个小优化  
            {
                for (int bitIndex = 0; bitIndex < 8; bitIndex++)
                {
                    if ((b & 1 << bitIndex) != 0)
                    {
                        result.Add(byteIndex * 8 + bitIndex);
                    }
                }
            }
        }
        var lastByteBitLength = bitLength % 8; // 计算最后一个字节的位数
        for (int bitIndex = 0; bitIndex < lastByteBitLength; bitIndex++)
        {
            if ((buffer[fullByteLength + offset] & 1 << bitIndex) != 0)
            {
                result.Add(fullByteLength * 8 + bitIndex);
            }
        }
        return result;
    }

    internal static async Task ProcessRead(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        onBuffer(buffer.Bytes);
    }

    internal static async Task ProcessRead(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        await onBuffer(buffer.Bytes);
    }

    internal static async Task<TResult> ProcessRead<TResult>(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task<TResult>> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        return await onBuffer(buffer.Bytes);
    }

    internal static async Task<TResult> ProcessRead<TResult>(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], TResult> onBuffer)
    {
        var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        return onBuffer(buffer.Bytes);
    }

    internal static async Task ProcessReadWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[], int> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        if (cacheOffset < 0 || cacheOffset > bufferSize)
        {
            Console.WriteLine($"cacheOffset={cacheOffset},address - cacheAddress={address - cacheAddress},cacheAddress={cacheAddress},cacheSize={cacheSize},address={address},bufferSize={bufferSize}");
            throw new ArgumentOutOfRangeException(nameof(address), "The address is out of the cache range.");
        }
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await provider.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        onBuffer(cache, cacheOffset);
    }

    internal static async Task ProcessReadWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, Task> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        if (cacheOffset < 0 || cacheOffset > bufferSize)
        {
            Console.WriteLine($"cacheOffset={cacheOffset},address - cacheAddress={address - cacheAddress},cacheAddress={cacheAddress},cacheSize={cacheSize},address={address},bufferSize={bufferSize}");
            throw new ArgumentOutOfRangeException(nameof(address), "The address is out of the cache range.");
        }
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await provider.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        await onBuffer(cache, cacheOffset);
    }

    internal static async Task ProcessWrite(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        onBuffer(buffer.Bytes);
        await address.BeginWrite(semaphore);
        await provider.FileStream.WriteAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndWrite(semaphore);
    }

    internal static async Task ProcessWrite(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await onBuffer(buffer.Bytes);
        await address.BeginWrite(semaphore);
        await provider.FileStream.WriteAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndWrite(semaphore);
    }

    internal static async Task ProcessWriteSpan(LayoutProvider provider, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        var buffer = await provider.Cache.BytesCache.Using(spanSize);
        onBuffer(buffer.Bytes);
        await address.BeginWrite(semaphore);
        await provider.FileStream.WriteAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
        await address.EndWrite(semaphore);
    }

    internal static async Task ProcessWriteSpan(LayoutProvider provider, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(spanSize);
        await onBuffer(buffer.Bytes);
        await address.BeginWrite(semaphore);
        await provider.FileStream.WriteAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
        await address.EndWrite(semaphore);
    }

    internal static async Task ProcessWriteWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[], int> onBuffer)
    {
        await address.BeginWrite(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool _);
        onBuffer(cache, cacheOffset);
        async Task func()
        {
            await provider.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
        _ = func();
    }

    internal static async Task ProcessWriteWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, Task> onBuffer)
    {
        await address.BeginWrite(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool _);
        await onBuffer(cache, cacheOffset);
        async Task func()
        {
            await provider.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
        _ = func();
    }

    internal static async Task ProcessUpdate(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], bool> onBuffer)
    {
        using var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        if (onBuffer(buffer.Bytes))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address, buffer.Bytes, 0, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdate(LayoutProvider provider, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task<bool>> onBuffer)
    {
        var buffer = await provider.Cache.BytesCache.Using(bufferSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address, buffer.Bytes, 0, bufferSize);
        await address.EndRead(semaphore);
        if (await onBuffer(buffer.Bytes))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address, buffer.Bytes, 0, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdateSpan(LayoutProvider provider, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task<bool>> onBuffer)
    {
        var buffer = await provider.Cache.BytesCache.Using(spanSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
        await address.EndRead(semaphore);
        if (await onBuffer(buffer.Bytes))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdateSpan(LayoutProvider provider, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], bool> onBuffer)
    {
        var buffer = await provider.Cache.BytesCache.Using(spanSize);
        await address.BeginRead(semaphore);
        await provider.FileStream.ReadAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
        await address.EndRead(semaphore);
        if (onBuffer(buffer.Bytes))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address + spanOffset, buffer.Bytes, 0, spanSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdateWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, bool> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await provider.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        if (onBuffer(cache, cacheOffset))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdateWithCache(LayoutProvider provider, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, Task<bool>> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        provider.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await provider.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        if (await onBuffer(cache, cacheOffset))
        {
            await address.BeginWrite(semaphore);
            await provider.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static int FindMaxRecordCount(int recordSize, int blockSize, int blockHeaderSize)
    {
        // 从一个可能的最大值开始迭代  
        int recordCount = (blockSize - blockHeaderSize) / recordSize;

        // 向下迭代直到找到满足条件的最大值  
        while (true)
        {
            if (blockHeaderSize + recordCount * recordSize + (int)Math.Ceiling(recordCount * 0.125) <= blockSize)
            {
                return recordCount; // 满足条件，返回当前recordCount  
            }
            recordCount--; // 不满足条件，递减recordCount  
            if (recordCount < 0)
            {
                throw new ArgumentException("Block size is too small to accommodate even one record.");
            }
        }
    }
}
