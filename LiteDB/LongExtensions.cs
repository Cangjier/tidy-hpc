using TidyHPC.Semaphores;

namespace TidyHPC.LiteDB;
internal static class LongExtensions
{
    internal static async Task WaitAsync(this long self,params SemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.WaitAsync(self);
        }
    }

    internal static async Task ReleaseAsync(this long self, params SemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.ReleaseAsync(self);
        }
    }

    internal static async Task BeginRead(this long self, params ReaderWriterSemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.BeginRead(self);
        }
    }

    internal static async Task EndRead(this long self, params ReaderWriterSemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.EndRead(self);
        }
    }

    internal static async Task BeginWrite(this long self, params ReaderWriterSemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.BeginWrite(self);
        }
    }

    internal static async Task EndWrite(this long self, params ReaderWriterSemaphorePool<long>[] pools)
    {
        if (pools.Length == 0)
        {
            throw new ArgumentException("At least one pool is required.", nameof(pools));
        }
        foreach (var pool in pools)
        {
            await pool.EndWrite(self);
        }
    }
}
