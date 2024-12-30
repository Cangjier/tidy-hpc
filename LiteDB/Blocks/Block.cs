using TidyHPC.Semaphores;

namespace TidyHPC.LiteDB.Blocks;

/// <summary>
/// 基础Block
/// </summary>
public abstract class Block
{
    /// <summary>
    /// Constructor
    /// </summary>
    public Block()
    {
        RecordVisitor = new RecordVisitor(this);
        RecordRegionVisitor = new(this);
    }

    #region Props

    /// <summary>
    /// 块的首地址
    /// </summary>
    public long Address { get; protected set; }

    /// <summary>
    /// 单个记录的大小
    /// </summary>
    public int RecordSize { get; protected set; }

    /// <summary>
    /// 块的大小
    /// </summary>
    public int BlockSize { get; protected set; }

    #endregion

    #region Const
    /// <summary>
    /// 记录的数量是通过块的大小计算的
    /// </summary>
    public int RecordCount { get; protected set; }

    /// <summary>
    /// 第一个记录的地址
    /// </summary>
    public long FirstRecordAddress { get; set; }

    /// <summary>
    /// 记录区域的大小
    /// </summary>
    internal int RecordRegionSize { get; set; }

    /// <summary>
    /// Address + BlockSize 
    /// </summary>
    internal long BoundarySize => Address + BlockSize;
    #endregion

    #region 设置和初始化
    /// <summary>
    /// 设置块的信息
    /// </summary>
    /// <param name="address">块的首地址</param>
    /// <param name="recordSize">单个记录的大小</param>
    /// <param name="blockSize">块的大小</param>
    public abstract void SetAddress(long address, int recordSize, int blockSize);

    /// <summary>
    /// 初始化块的数据
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public abstract Task Initialize(Database table);
    #endregion

    #region Vistor
    /// <summary>
    /// 记录访问器
    /// </summary>
    public RecordVisitor RecordVisitor { get; }

    /// <summary>
    /// 记录区域的访问器
    /// </summary>
    public RecordRegionVisitor RecordRegionVisitor { get; }
    #endregion
}

internal static class BlockUtil
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

    internal static async Task ProcessRead(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address, buffer, 0, bufferSize);
        await address.EndRead(semaphore);
        onBuffer(buffer);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessRead(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[],Task> onBuffer)
    {
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address, buffer, 0, bufferSize);
        await address.EndRead(semaphore);
        await onBuffer(buffer);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessReadWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[], int> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        if (cacheOffset < 0||cacheOffset>bufferSize)
        {
            Console.WriteLine($"cacheOffset={cacheOffset},address - cacheAddress={address - cacheAddress},cacheAddress={cacheAddress},cacheSize={cacheSize},address={address},bufferSize={bufferSize}");
            throw new ArgumentOutOfRangeException(nameof(address), "The address is out of the cache range.");
        }
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await table.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        onBuffer(cache, cacheOffset);
    }

    internal static async Task ProcessReadWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int,Task> onBuffer)
    {
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        if (cacheOffset < 0 || cacheOffset > bufferSize)
        {
            Console.WriteLine($"cacheOffset={cacheOffset},address - cacheAddress={address - cacheAddress},cacheAddress={cacheAddress},cacheSize={cacheSize},address={address},bufferSize={bufferSize}");
            throw new ArgumentOutOfRangeException(nameof(address), "The address is out of the cache range.");
        }
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await table.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        await onBuffer(cache, cacheOffset);
    }

    //internal static void CheckInRange(long address,long rangeCenter,int range)
    //{
    //    if(address== 4806061)
    //    {

    //    }
    //    if(rangeCenter-range < address && address < rangeCenter + range)
    //    {
    //        Console.WriteLine($"address={address},rangeCenter={rangeCenter},range={range}");
    //    }    
    //}

    internal static async Task ProcessWrite(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        onBuffer(buffer);
        await address.BeginWrite(semaphore);
        await table.FileStream.WriteAsync(address, buffer, 0, bufferSize);
        await address.EndWrite(semaphore);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessWrite(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[],Task> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        await onBuffer(buffer);
        await address.BeginWrite(semaphore);
        await table.FileStream.WriteAsync(address, buffer, 0, bufferSize);
        await address.EndWrite(semaphore);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessWriteSpan(Database table, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[]> onBuffer)
    {
        //CheckInRange(address+spanOffset, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(spanSize);
        onBuffer(buffer);
        await address.BeginWrite(semaphore);
        await table.FileStream.WriteAsync(address + spanOffset, buffer, 0, spanSize);
        await address.EndWrite(semaphore);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessWriteSpan(Database table, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[],Task> onBuffer)
    {
        //CheckInRange(address+spanOffset, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(spanSize);
        await onBuffer(buffer);
        await address.BeginWrite(semaphore);
        await table.FileStream.WriteAsync(address + spanOffset, buffer, 0, spanSize);
        await address.EndWrite(semaphore);
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessWriteWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Action<byte[], int> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        await address.BeginWrite(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool _);
        onBuffer(cache, cacheOffset);
        async Task func()
        {
            await table.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
        _ = func();
    }

    internal static async Task ProcessWriteWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int,Task> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        await address.BeginWrite(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool _);
        await onBuffer(cache, cacheOffset);
        async Task func()
        {
            await table.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
        _ = func();
    }

    internal static async Task ProcessUpdate(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], bool> onBuffer)
    {
        //table.DebugLogger.WriteLine($"ProcessUpdate {address} {bufferSize}");
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address, buffer, 0, bufferSize);
        await address.EndRead(semaphore);
        if (onBuffer(buffer))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address, buffer, 0, bufferSize);
            await address.EndWrite(semaphore);
        }
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessUpdate(Database table, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task<bool>> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(bufferSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address, buffer, 0, bufferSize);
        await address.EndRead(semaphore);
        if (await onBuffer(buffer))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address, buffer, 0, bufferSize);
            await address.EndWrite(semaphore);
        }
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessUpdateSpan(Database table, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], Task<bool>> onBuffer)
    {
        //CheckInRange(address+spanOffset, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(spanSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address + spanOffset, buffer, 0, spanSize);
        await address.EndRead(semaphore);
        if (await onBuffer(buffer))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address + spanOffset, buffer, 0, spanSize);
            await address.EndWrite(semaphore);
        }
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessUpdateSpan(Database table, long address, int spanOffset, int spanSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], bool> onBuffer)
    {
        //CheckInRange(address+spanOffset, 4806061, HashNode<Int64Value>.Size);
        var buffer = await table.Cache.DequeueBuffer(spanSize);
        await address.BeginRead(semaphore);
        await table.FileStream.ReadAsync(address + spanOffset, buffer, 0, spanSize);
        await address.EndRead(semaphore);
        if (onBuffer(buffer))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address + spanOffset, buffer, 0, spanSize);
            await address.EndWrite(semaphore);
        }
        table.Cache.EnqueueBuffer(buffer);
    }

    internal static async Task ProcessUpdateWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, bool> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await table.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        if (onBuffer(cache, cacheOffset))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static async Task ProcessUpdateWithCache(Database table, long cacheAddress, int cacheSize, long address, int bufferSize, ReaderWriterSemaphorePool<long> semaphore, Func<byte[], int, Task<bool>> onBuffer)
    {
        //CheckInRange(address, 4806061, HashNode<Int64Value>.Size);
        await address.BeginRead(semaphore);
        var cacheOffset = (int)(address - cacheAddress);
        table.Cache.IOCache.UseCache(cacheAddress, cacheSize, out byte[] cache, out bool first);
        if (first)
        {
            await table.FileStream.ReadAsync(cacheAddress, cache, 0, cacheSize);
        }
        await address.EndRead(semaphore);
        if (await onBuffer(cache, cacheOffset))
        {
            await address.BeginWrite(semaphore);
            await table.FileStream.WriteAsync(address, cache, cacheOffset, bufferSize);
            await address.EndWrite(semaphore);
        }
    }

    internal static int FindMaxRecordCount(int recordSize, int blockSize,int blockHeaderSize)
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

/// <summary>
/// 记录访问器
/// </summary>
/// <param name="block"></param>
public class RecordVisitor(Block block)
{
    /// <summary>
    /// 访问的块
    /// </summary>
    public Block Block { get; } = block;

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, long address, Action<byte[]> onBuffer)
    {
        await BlockUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, long address, Func<byte[],Task> onBuffer)
    {
        await BlockUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task ReadByIndex(Database table, int index, Action<byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await BlockUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 读取记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task ReadByIndex(Database table, int index, Action<long,byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await BlockUtil.ProcessRead(table, address, Block.RecordSize, table.RecordSemaphore, buffer => onBuffer(address, buffer));
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, long address, Action<byte[]> onBuffer)
    {
        await BlockUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, long address, Func<byte[],Task> onBuffer)
    {
        await BlockUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteSpan(Database table, long address, int spanOffset, int spanSize, Action<byte[]> onBuffer)
    {
        await BlockUtil.ProcessWriteSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteSpan(Database table, long address, int spanOffset, int spanSize, Func<byte[], Task> onBuffer)
    {
        await BlockUtil.ProcessWriteSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 写入记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task WriteByIndex(Database table, int index, Action<byte[]> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await BlockUtil.ProcessWrite(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, long address, Func<byte[], bool> onBuffer)
    {
        await BlockUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, long address, Func<byte[], Task<bool>> onBuffer)
    {
        await BlockUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <returns></returns>
    public async Task UpdateSpan(Database table, long address,int spanOffset,int spanSize, Func<byte[], Task<bool>> onBuffer)
    {
        await BlockUtil.ProcessUpdateSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录，onBuffer传入的数据是局部数据
    /// </summary>
    /// <param name="table"></param>
    /// <param name="address"></param>
    /// <param name="onBuffer"></param>
    /// <param name="spanOffset"></param>
    /// <param name="spanSize"></param>
    /// <returns></returns>
    public async Task UpdateSpan(Database table, long address, int spanOffset, int spanSize, Func<byte[], bool> onBuffer)
    {
        await BlockUtil.ProcessUpdateSpan(table, address, spanOffset, spanSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task UpdateByIndex(Database table, int index, Func<byte[], bool> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await BlockUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }

    /// <summary>
    /// 更新记录
    /// </summary>
    /// <param name="table"></param>
    /// <param name="index"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task UpdateByIndex(Database table, int index, Func<byte[], Task<bool>> onBuffer)
    {
        var address = Block.FirstRecordAddress + index * Block.RecordSize;
        await BlockUtil.ProcessUpdate(table, address, Block.RecordSize, table.RecordSemaphore, onBuffer);
    }
}
