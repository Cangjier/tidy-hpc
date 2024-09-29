using TidyHPC.Common;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.Arrays;

/// <summary>
/// Array Record Processor
/// </summary>
internal class ArrayProcessor<TValue> : IDisposable
    where TValue : IValue<TValue>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="db"></param>
    /// <param name="firstArrayAddress"></param>
    public ArrayProcessor(Database db, long firstArrayAddress)
    {
        Database = db;
        FirstArrayAddress = firstArrayAddress;
    }

    /// <summary>
    /// Database
    /// </summary>
    public Database Database { get; private set; }

    /// <summary>
    /// First Array Address
    /// </summary>
    public long FirstArrayAddress { get; }

    /// <summary>
    /// Get the length of the array
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetLength()
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginRead(FirstArrayAddress);
        try
        {
            int result = 0;
            block.SetByRecordAddress(FirstArrayAddress, ArrayRecord<TValue>.Size);
            await block.RecordVisitor.Read(Database, FirstArrayAddress, buffer =>
            {
                ArrayRecord<TValue> record = new();
                record.Read(buffer, 0);
                result = record.Length;
            });
            return result;
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndRead(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
    }

    /// <summary>
    /// Add a long value to the array
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Add(TValue value)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginWrite(FirstArrayAddress);
        try
        {
            bool isWrited = false;
            var lastArrayAddress = FirstArrayAddress;
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    throw new Exception("lastArrayAddress is 0");
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Update(Database, currentArrayAddress, buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        if (record.Values[i].IsEmpty())
                        {
                            record.Values[i] = value;
                            isWrited = true;
                            break;
                        }
                    }
                    if (isWrited)
                    {
                        record.Write(buffer, 0);
                        return true;
                    }
                    else
                    {
                        lastArrayAddress = record.NextAddress;
                        return false;
                    }
                });
                if (isWrited)
                {
                    break;
                }
                else if (lastArrayAddress == 0)
                {
                    // 新建一个数组
                    var newArrayAddress = await Database.AllocateRecord(ArrayRecord<TValue>.InterfaceName);
                    await block.RecordVisitor.Update(Database, currentArrayAddress, buffer =>
                    {
                        ArrayRecord<TValue> record = new();
                        record.Read(buffer, 0);
                        record.NextAddress = newArrayAddress;
                        record.Write(buffer, 0);
                        return true;
                    });
                    lastArrayAddress = newArrayAddress;
                }
            }
            block.SetByRecordAddress(FirstArrayAddress, ArrayRecord<TValue>.Size);
            await block.RecordVisitor.Update(Database, FirstArrayAddress, buffer =>
            {
                ArrayRecord<TValue> record = new();
                record.Read(buffer, 0);
                record.Length++;
                record.Write(buffer, 0);
                return true;
            });
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndWrite(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }

    }

    /// <summary>
    /// If the array contains the value, return true, otherwise return false
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Contains(TValue value)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginRead(FirstArrayAddress);
        try
        {
            bool result = false;
            var lastArrayAddress = FirstArrayAddress;
            while (true)
            {

                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Read(Database, currentArrayAddress, buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        if (record.Values[i] == value)
                        {
                            result = true;
                            break;
                        }
                    }
                    lastArrayAddress = record.NextAddress;
                });
            }
            return result;
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndRead(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
       
    }

    public async Task Get(Action<TValue> onValue)
        => await Get(value =>
        {
            onValue(value);
            return Task.CompletedTask;
        });

    public async Task Get(Func<TValue, Task> onValue)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginRead(FirstArrayAddress);
        try
        {
            var lastArrayAddress = FirstArrayAddress;
            TValue[] tempArray = new TValue[ArrayRecord<TValue>.ArrayLength];
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Read(Database, currentArrayAddress, buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        tempArray[i] = record.Values[i];
                    }
                    lastArrayAddress = record.NextAddress;
                });
                for (int i = 0; i < tempArray.Length; i++)
                {
                    if (tempArray[i].IsEmpty() == false)
                    {
                        await onValue(tempArray[i]);
                    }
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndRead(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
    }

    public async Task<bool> TryFind(Func<TValue, Task<bool>> equals,Ref<TValue> outValue)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginRead(FirstArrayAddress);
        try
        {
            bool isFinded = false;
            var lastArrayAddress = FirstArrayAddress;
            TValue[] tempArray = new TValue[ArrayRecord<TValue>.ArrayLength];
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Read(Database, currentArrayAddress, buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        tempArray[i] = record.Values[i];
                    }
                    lastArrayAddress = record.NextAddress;
                });
                for (int i = 0; i < tempArray.Length; i++)
                {
                    if (tempArray[i].IsEmpty() == false)
                    {
                        if (await equals(tempArray[i]))
                        {
                            isFinded = true;
                            outValue.Value = tempArray[i];
                            break;
                        }
                    }
                }
                if (isFinded) break;
            }
            return isFinded;
        }
        catch
        {
            throw;
        }
        finally
        {   
            await Database.ArraySemaphore.EndRead(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
    }

    public async Task<bool> Replace(Func<TValue, Task<bool>> equals,Func<TValue, Task<TValue>>? onReplace)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginWrite(FirstArrayAddress);
        try
        {
            bool isReplaced = false;
            var lastArrayAddress = FirstArrayAddress;
            TValue[] tempArray = new TValue[ArrayRecord<TValue>.ArrayLength];
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Update(Database, currentArrayAddress, async buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        if (await equals(record.Values[i]))
                        {
                            if (onReplace != null)
                            {
                                record.Values[i] = await onReplace(record.Values[i]);
                            }
                            isReplaced = true;
                            break;
                        }
                    }
                    lastArrayAddress = record.NextAddress;
                    return isReplaced;
                });
                if (isReplaced) break;
            }
            return isReplaced;
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndWrite(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
    }

    /// <summary>
    /// Remove the value from the array
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Remove(TValue value)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginWrite(FirstArrayAddress);
        try
        {
            bool isDeleted = false;
            var lastArrayAddress = FirstArrayAddress;
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Update(Database, currentArrayAddress, buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        if (record.Values[i] == value)
                        {
                            record.Values[i].SetEmpty();
                            isDeleted = true;
                            break;
                        }
                    }
                    lastArrayAddress = record.NextAddress;
                    return isDeleted;
                });
                if (isDeleted) break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndWrite(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
    }

    public async Task Remove(Func<TValue,Task<bool>> equals)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await Database.ArraySemaphore.BeginWrite(FirstArrayAddress);
        try
        {
            bool isDeleted = false;
            var lastArrayAddress = FirstArrayAddress;
            while (true)
            {
                if (lastArrayAddress == 0)
                {
                    break;
                }
                var currentArrayAddress = lastArrayAddress;
                block.SetByRecordAddress(currentArrayAddress, ArrayRecord<TValue>.Size);
                await block.RecordVisitor.Update(Database, currentArrayAddress, async buffer =>
                {
                    ArrayRecord<TValue> record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.Values!.Length; i++)
                    {
                        if (record.Values[i].IsEmpty()) continue;
                        if (await equals(record.Values[i]))
                        {
                            record.Values[i].SetEmpty();
                            isDeleted = true;
                            break;
                        }
                    }
                    if (isDeleted)
                    {
                        record.Write(buffer, 0);
                    }
                    lastArrayAddress = record.NextAddress;
                    return isDeleted;
                });
                if (isDeleted) break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            await Database.ArraySemaphore.EndWrite(FirstArrayAddress);
            Database.Cache.StatisticalBlockPool.Enqueue(block);
        }
        
    }

    public async Task Remove(Func<TValue,bool> equals)
        => await Remove(value =>
        {
            return Task.FromResult(equals(value));
        });

    public void Dispose()
    {
        Database = null!;
    }
}