namespace TidyHPC.Queues;

/// <summary>
/// 循环缓存区
/// </summary>
public class CircularBuffer<T>(int size)
{
    private T[] Buffer { get; } = new T[size];
    private int Index { get; set; } = 0;
    private bool IsFull { get; set; } = false;

    private object LockObject { get; } = new object();

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="bytes">要写入的数据</param>
    public void Write(ReadOnlySpan<T> bytes)
    {
        lock (LockObject)
        {
            // 当前缓存区已经被填满
            var bufferLeftLength = Index;
            var bufferRightLength = Buffer.Length - Index;
            // 判断bytes的length是否大于bufferLengh
            var toWriteBytes = bytes;
            if (bytes.Length > Buffer.Length)
            {
                toWriteBytes = bytes.Slice(bytes.Length - Buffer.Length);
            }
            // 判断toWriteBytes的length是否大于bufferRightLength
            if (toWriteBytes.Length > bufferRightLength)
            {
                var toWriteRightBytes = toWriteBytes.Slice(0, bufferRightLength);
                var toWriteLeftBytes = toWriteBytes.Slice(bufferRightLength);
                // 将toWriteLeftBytes写入Buffer
                toWriteLeftBytes.CopyTo(Buffer.AsSpan(0, toWriteLeftBytes.Length));
                // 将toWriteRightBytes写入Buffer
                toWriteRightBytes.CopyTo(Buffer.AsSpan(Index, toWriteRightBytes.Length));
                Index = toWriteLeftBytes.Length;
                IsFull = true;
            }
            else
            {
                var toWriteRightBytes = toWriteBytes;
                // 将toWriteRightBytes写入Buffer
                toWriteRightBytes.CopyTo(Buffer.AsSpan(Index, toWriteRightBytes.Length));
                Index = Index + toWriteRightBytes.Length;
            }
        }
    }

    /// <summary>
    /// 将缓存区中的数据转换为数组
    /// </summary>
    /// <returns>转换后的数组</returns>
    public T[] ToArray()
    {
        lock (LockObject)
        {
            if (IsFull)
            {
                T[] result = new T[Buffer.Length];
                if (Index == 0)
                {
                    Buffer.CopyTo(result.AsSpan(0, result.Length));
                }
                else
                {
                    Buffer.AsSpan(Index, Buffer.Length - Index).CopyTo(result.AsSpan(0, Buffer.Length - Index));
                    Buffer.AsSpan(0, Index).CopyTo(result.AsSpan(Buffer.Length - Index, Index));
                }
                return result;
            }
            else
            {
                return Buffer.AsSpan(0, Index).ToArray();
            }
        }
    }

    /// <summary>
    /// 清空缓存区
    /// </summary>
    public void Clear()
    {
        lock (LockObject)
        {
            Index = 0;
            IsFull = false;
        }
    }
}