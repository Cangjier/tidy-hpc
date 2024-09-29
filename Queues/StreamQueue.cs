using System.Collections.Concurrent;
using System.Text;

namespace TidyHPC.Queues;

/// <summary>
/// 流队列
/// </summary>
public class StreamQueue<TStream> : WaitQueue<TStream>
    where TStream : Stream
{
    private async Task Process(Action<TStream> onStream)
    {
        var stream = await Dequeue();
        if (stream == null)
        {
            throw new Exception("stream is null");
        }
        try
        {
            onStream(stream);
        }
        finally
        {
            Enqueue(stream);
        }
    }

    /// <summary>
    /// 取出一个FileStream进行相应处理
    /// </summary>
    /// <param name="onStream"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Process(Func<TStream, Task> onStream)
    {
        var stream = await Dequeue();
        if (stream == null)
        {
            throw new Exception("stream is null");
        }
        try
        {
            await onStream(stream);
        }
        catch
        {
            Console.WriteLine("Process Error");
            throw;
        }
        finally
        {
            Enqueue(stream);
        }
    }

    /// <summary>
    /// 读取流
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferOffset"></param>
    /// <param name="bufferCount"></param>
    /// <returns></returns>
    public async Task ReadAsync(long offset, byte[] buffer, int bufferOffset, int bufferCount)
    {
        await Process(async stream =>
        {
            await stream.FlushAsync();
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.ReadAsync(buffer,bufferOffset, bufferCount);
        });
    }

    /// <summary>
    /// 读取流
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public async Task ReadAsync(long offset, byte[] buffer)
    {
        await Process(async stream =>
        {
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.ReadAsync(buffer);
        });
    }

    /// <summary>
    /// 写入流
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferOffset"></param>
    /// <param name="bufferCount"></param>
    /// <returns></returns>
    public async Task WriteAsync(long offset, byte[] buffer, int bufferOffset, int bufferCount)
    {
        await Process(async stream =>
        {
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.WriteAsync(buffer, bufferOffset, bufferCount);
            await stream.FlushAsync();
        });
    }

    /// <summary>
    /// 获取流的长度
    /// </summary>
    /// <returns></returns>
    public async Task<long> GetLengthAsync()
    {
        long result = 0;
        await Process(stream =>
        {
            result = stream.Length;
        });
        return result;
    }

    /// <summary>
    /// 填充字节
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    private void FillBytes(byte[] buffer, byte value)
    {
        int length = buffer.Length;
        for (int i = 0; i < length; i++)
        {
            buffer[i] = value;
        }
    }

    /// <summary>
    /// 获取字符串的字节长度
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int GetByteCount(string value) => Util.UTF8.GetByteCount(value);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public async Task WriteBytesAsync(long offset, byte[] buffer)
    {
        await Process(async stream =>
        {
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
        });
    }

    /// <summary>
    /// 写入固定长度数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteFixBytesAsync(long offset, byte[] buffer, int fixCount, byte paddingByte)
    {
        await Process(async stream =>
        {
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.WriteAsync(buffer);
            if (fixCount - buffer.Length > 0)
            {
                byte[] padding = new byte[fixCount - buffer.Length];
                FillBytes(padding, paddingByte);
                await stream.WriteAsync(padding);
                await stream.FlushAsync();
            }
        });
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteStringAsync(long offset, string value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, Util.UTF8.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteIntAsync(long offset, int value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, BitConverter.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteFloatAsync(long offset, float value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, BitConverter.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteDoubleAsync(long offset, double value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, BitConverter.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteBooleanAsync(long offset, bool value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, BitConverter.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteLongAsync(long offset, long value, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, BitConverter.GetBytes(value), fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="buffer"></param>
    /// <param name="fixCount"></param>
    /// <param name="paddingByte"></param>
    /// <returns></returns>
    public async Task WriteBytesAsync(long offset, byte[] buffer, int fixCount, byte paddingByte) => await WriteFixBytesAsync(offset, buffer, fixCount, paddingByte);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WriteStringAsync(long offset, string value) => await WriteBytesAsync(offset, Util.UTF8.GetBytes(value));

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WriteIntAsync(long offset, int value) => await WriteBytesAsync(offset, BitConverter.GetBytes(value));

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WritFloateAsync(long offset, float value) => await WriteBytesAsync(offset, BitConverter.GetBytes(value));

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WriteDoubleAsync(long offset, double value) => await WriteBytesAsync(offset, BitConverter.GetBytes(value));

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WriteBooleanAsync(long offset, bool value) => await WriteBytesAsync(offset, BitConverter.GetBytes(value));

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task WriteLongAsync(long offset, long value) => await WriteBytesAsync(offset, BitConverter.GetBytes(value));

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<byte[]> ReadBytesAsync(long offset, int count)
    {
        byte[] buffer = new byte[count];
        await Process(async stream =>
        {
            stream.Seek(offset, SeekOrigin.Begin);
            await stream.ReadAsync(buffer);
        });
        return buffer;
    }

    /// <summary>
    /// 读取Long
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<long> ReadLongAsync(long offset) => BitConverter.ToInt64(await ReadBytesAsync(offset, sizeof(long)));

    /// <summary>
    /// 读取Int
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<int> ReadIntAsync(long offset) => BitConverter.ToInt32(await ReadBytesAsync(offset, sizeof(int)));

    /// <summary>
    /// 读取Float
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<float> ReadFloatAsync(long offset) => BitConverter.ToSingle(await ReadBytesAsync(offset, sizeof(float)));

    /// <summary>
    /// 读取Double
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<double> ReadDoubleAsync(long offset) => BitConverter.ToDouble(await ReadBytesAsync(offset, sizeof(double)));

    /// <summary>
    /// 读取Boolean
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public async Task<bool> ReadBooleanAsync(long offset) => BitConverter.ToBoolean(await ReadBytesAsync(offset, sizeof(bool)));

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <returns></returns>
    public override Task ReleaseResources()
    {
        return base.ReleaseResources();
    }
}
