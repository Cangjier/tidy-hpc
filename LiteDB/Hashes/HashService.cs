using System.ComponentModel;
using System.Security.Cryptography;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Hashes;

/// <summary>
/// Hash相关的服务
/// </summary>
public class HashService
{
    private static WaitQueue<MD5> MD5Queue = new([MD5.Create(), MD5.Create()])
    {
        OnDequeueStart = MD5QueueOnDequeue
    };

    private static async Task MD5QueueOnDequeue()
    {
        await Task.CompletedTask;
        if(MD5Queue.CurrentCount>0)
        {
            return;
        }
        MD5Queue.Enqueue(MD5.Create());
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(byte[] bytes)
    {
        var md5 = await MD5Queue.Dequeue();
        byte[] hash = md5.ComputeHash(bytes);
        MD5Queue.Enqueue(md5);
        return BitConverter.ToUInt64(hash, 0);
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(string value)
    {
        return await GetHashCode(Util.UTF8.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(int value)
    {
        return await GetHashCode(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(long value)
    {
        return await GetHashCode(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(float value)
    {
        return await GetHashCode(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(double value)
    {
        return await GetHashCode(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(bool value)
    {
        return await GetHashCode(BitConverter.GetBytes(value));
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(byte value)
    {
        return await GetHashCode([value]);
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(Guid value)
    {
        return await GetHashCode(value.ToByteArray());
    }

    /// <summary>
    /// 计算Hash值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task<ulong> GetHashCode(DateTime value)
    {
        return await GetHashCode(BitConverter.GetBytes(value.Ticks));
    }

}
