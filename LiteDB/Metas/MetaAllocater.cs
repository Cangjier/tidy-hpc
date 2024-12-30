using TidyHPC.LiteJson;
using TidyHPC.Locks;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 
/// </summary>
internal class MetaAllocater
{
    public MetaAllocater(Database db,string typeName, long metaRecordAddress)
    {
        Database = db;
        TypeName = typeName;
        MetaRecordAddress = metaRecordAddress;
        BlockQueue = new()
        {
            OnDequeueStart = OnDequeue,
        };
    }

    private Database Database { get; }

    public string TypeName { get; }

    /// <summary>
    /// 目标记录地址
    /// </summary>
    public long MetaRecordAddress { get;}

    private WaitQueue<long> BlockQueue { get; }

    private Lock<List<long>> UsedBlockAddresses { get; } = new(new());

    /// <summary>
    /// 缓存数据，所有块的地址，仅用于参考
    /// <para>1. 当申请块时，会将该数据加入</para>
    /// <para>2. 当调用GetBlockAddresses时，会将数据加入</para>
    /// <para>3. 当ContainsBlockAddress从MetaRecord遍历时，并比对成功，会将数据加入</para>
    /// </summary>
    private Lock<HashSet<long>> CacheBlockAddresses { get; } = new(new());

    private int _RecordSize = 0;

    private async Task<int> GetRecordSize()
    {
        if (_RecordSize == 0)
        {
            var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
            metaBlock.SetByRecordAddress(MetaRecordAddress);
            await metaBlock.RecordVisitor.Read(Database, MetaRecordAddress, buffer =>
            {
                MetaRecord record = new();
                record.Read(buffer, 0);
                _RecordSize = record.RecordSize;
            });
            Database.Cache.MetaBlockPool.Enqueue(metaBlock);
        }
        return _RecordSize;
    }

    /// <summary>
    /// 将地址添加到MetaRecord中
    /// </summary>
    /// <param name="blockAddress"></param>
    /// <returns></returns>
    private async Task AddBlockAddressToMetaRecord(long blockAddress)
    {
        var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
        try
        {
            long lastMetaReocrdAddress = MetaRecordAddress;
            bool isWrited = false;
            while (true)
            {
                var currentMetaRecordAddress = lastMetaReocrdAddress;
                metaBlock.SetByRecordAddress(currentMetaRecordAddress);
                //Database.DebugLogger.WriteLine($"SetBlockAddresses(AddBlockAddressToMetaRecord) {currentMetaRecordAddress} {blockAddress}");
                await metaBlock.RecordVisitor.Update(Database, currentMetaRecordAddress, buffer =>
                {
                    MetaRecord record = new();
                    record.Read(buffer, 0,Database);
                    for (int i = 0; i < record.BlockAddresses!.Length; i++)
                    {
                        if (record.BlockAddresses[i] == 0)
                        {
                            record.BlockAddresses[i] = blockAddress;
                            isWrited = true;
                            break;
                        }
                    }
                    if (isWrited)
                    {
                        record.Write(buffer, 0, Database);
                    }
                    lastMetaReocrdAddress = record.NextMetaRecordAddress;
                    return isWrited;
                });
                if (isWrited)
                {
                    break;
                }
                else if (lastMetaReocrdAddress == 0)
                {
                    // 申请一个新的MetaRecord
                    var newMetaRecordAddress = await Database.AllocateRecord(Database.__META_RECORD__);
                    // 更新当前MetaRecord
                    await metaBlock.RecordVisitor.Update(Database, currentMetaRecordAddress, buffer =>
                    {
                        MetaRecord record = new();
                        record.Read(buffer, 0);
                        record.NextMetaRecordAddress = newMetaRecordAddress;
                        record.Write(buffer, 0);
                        return true;
                    });
                    // 更新新的MetaRecord
                    metaBlock.SetByRecordAddress(newMetaRecordAddress);
                    await metaBlock.RecordVisitor.Update(Database, newMetaRecordAddress, buffer =>
                    {
                        MetaRecord record = new();
                        record.Read(buffer, 0);
                        record.FirstMetaRecordAddress = MetaRecordAddress;
                        record.NextMetaRecordAddress = 0;
                        record.Write(buffer, 0);
                        return true;
                    });
                    lastMetaReocrdAddress = newMetaRecordAddress;
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.MetaBlockPool.Enqueue(metaBlock);
        }
    }

    private async Task<long> AllocateStatisticalBlock()
    {
        #region 申请统计块
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        var blockAddress = await Database.AllocateBlock();
        statisticalBlock.Set(blockAddress, await GetRecordSize());
        await statisticalBlock.Initialize(Database);
        Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        #endregion
        //Database.DebugLogger.WriteLine($"AllocateStatisticalBlock {TypeName} MetaRecordAddress({MetaRecordAddress}) {blockAddress}");
        await AddBlockAddressToMetaRecord(blockAddress);
        CacheBlockAddresses.Process(() =>
        {
            CacheBlockAddresses.Value.Add(blockAddress);
        });
        return blockAddress;
    }

    private async Task<long?> GetFreeBlockAddress()
    {
        var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        long result = 0;
        try
        {
            long lastMetaReocrdAddress = MetaRecordAddress;
            bool isFind = false;
            while (true)
            {
                if (lastMetaReocrdAddress == 0)
                {
                    break;
                }
                var currentMetaReocrdAddress = lastMetaReocrdAddress;
                metaBlock.SetByRecordAddress(currentMetaReocrdAddress);
                await metaBlock.RecordVisitor.Read(Database, currentMetaReocrdAddress, async buffer =>
                {
                    MetaRecord record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.BlockAddresses!.Length; i++)
                    {
                        if (record.BlockAddresses[i] == 0)
                        {
                            continue;
                        }
                        bool isUsed = false;
                        UsedBlockAddresses.Process((list) =>
                        {
                            isUsed = list.Contains(record.BlockAddresses[i]);
                        });
                        if (isUsed)
                        {
                            continue;
                        }
                        statisticalBlock.Set(record.BlockAddresses[i], await GetRecordSize());
                        bool containsUnused = await statisticalBlock.ContainsUnused(Database);
                        if (containsUnused)
                        {
                            result = record.BlockAddresses[i];
                            isFind = true;
                            break;
                        }
                    }
                    lastMetaReocrdAddress = record.NextMetaRecordAddress;
                });
                if (isFind)
                {
                    break;
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.MetaBlockPool.Enqueue(metaBlock);
            Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        }
        return result == 0 ? null : result;
    }

    private SemaphoreSlim OnDequeueSemaphoreSlim { get; } = new(1,1);

    private async Task OnDequeue()
    {
        await OnDequeueSemaphoreSlim.WaitAsync();
        try
        {
            if (BlockQueue.CurrentCount > 0)
            {
                return;
            }
            var blockAddress = await GetFreeBlockAddress();
            if (blockAddress == null)
            {
                blockAddress = await AllocateStatisticalBlock();
            }
            UsedBlockAddresses.Process(list =>
            {
                list.Add(blockAddress.Value);
            });
            BlockQueue.Enqueue(blockAddress.Value);
        }
        catch
        {
            throw;
        }
        finally
        {
            OnDequeueSemaphoreSlim.Release();
        }
    }

    private Lock<int> AllocateRecordUsingCount { get; } = new(0);

    private Lock<int> AllocateRecordTotalUsedCount { get; }= new(0);

    /// <summary>
    /// 申请一个Record
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<long> AllocateRecord()
    {
        long result;
        AllocateRecordUsingCount.Process(() =>
        {
            AllocateRecordUsingCount.Value++;
        });
        AllocateRecordTotalUsedCount.Process(() =>
        {
            AllocateRecordTotalUsedCount.Value++;
        });
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        try
        {
            while (true)
            {
                var blockAddress = await BlockQueue.Dequeue();
                statisticalBlock.Set(blockAddress, await GetRecordSize());
                if (await statisticalBlock.ContainsUnused(Database))
                {
                    var address = await statisticalBlock.AllocateRecord(Database);
                    //if(address == 4804685)
                    //{

                    //}
                    //用完之后，将地址放回队列
                    BlockQueue.Enqueue(blockAddress);
                    if (address.HasValue)
                    {
                        result = address.Value;
                        break;
                    }
                    else
                    {
                        var size = await statisticalBlock.GetUsedCount(Database);
                        Console.WriteLine($"{"MetaAllocater.AllocateRecord",-32},{$"{TypeName}/{blockAddress}",-32},{$"UsedCount={size}/{statisticalBlock.RecordCount}",-32}");
                        throw new Exception("allocate record faild");
                    }
                }
                else
                {
                    //该块已满，不进行使用
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
            AllocateRecordUsingCount.Process(() =>
            {
                AllocateRecordUsingCount.Value--;
            });
        }
        return result;
    }

    private Lock<int> AllocateHashTableUsedCount { get; } = new(0);

    /// <summary>
    /// 申请一个HashTable
    /// </summary>
    /// <returns></returns>
    public async Task<long> AllocateHashTable()
    {
        AllocateHashTableUsedCount.Process(() =>
        {
            AllocateHashTableUsedCount.Value++;
        });
        var hashTable = await Database.Cache.HashTablePool.Dequeue();
        var blockAddress = await Database.AllocateBlock();
        hashTable.SetAddress(blockAddress);
        await hashTable.Initialize(Database);
        Database.Cache.HashTablePool.Enqueue(hashTable);
        //Database.DebugLogger.WriteLine($"AllocateHashTable {TypeName} MetaRecordAddress({MetaRecordAddress}) {blockAddress}");
        await AddBlockAddressToMetaRecord(blockAddress);
        return blockAddress;
    }

    public async Task<long[]> GetBlockAddresses()
    {
        List<long> result = [];
        long lastMetaReocrdAddress = MetaRecordAddress;
        var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        try
        {
            while (true)
            {
                if (lastMetaReocrdAddress == 0)
                {
                    break;
                }
                var currentMetaReocrdAddress = lastMetaReocrdAddress;
                metaBlock.SetByRecordAddress(currentMetaReocrdAddress);
                await metaBlock.RecordVisitor.Read(Database, currentMetaReocrdAddress, buffer =>
                {
                    MetaRecord record = new();
                    record.Read(buffer, 0);
                    for (int i = 0; i < record.BlockAddresses!.Length; i++)
                    {
                        if (record.BlockAddresses[i] == 0)
                        {
                            continue;
                        }
                        result.Add(record.BlockAddresses[i]);
                    }
                    lastMetaReocrdAddress = record.NextMetaRecordAddress;
                });
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.MetaBlockPool.Enqueue(metaBlock);
            Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        }
        CacheBlockAddresses.Process(() =>
        {
            CacheBlockAddresses.Value.UnionWith(result);
        });
        return result.ToArray();
    }

    /// <summary>
    /// 判断是否包含指定地址的块
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<bool> ContainsBlockAddress(long address)
    {
        bool result = false;
        CacheBlockAddresses.Process(() =>
        {
            //Database.DebugLogger.WriteLine($"ContainsBlockAddress {TypeName} {address} cache {new Json(CacheBlockAddresses.Value.ToArray()).ToString(false)}");
            result = CacheBlockAddresses.Value.Contains(address);
        });
        if (result)
        {
            return result;
        }

        long lastMetaReocrdAddress = MetaRecordAddress;
        var metaBlock = await Database.Cache.MetaBlockPool.Dequeue();
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        try
        {
            while (true)
            {
                if (lastMetaReocrdAddress == 0)
                {
                    break;
                }
                var currentMetaReocrdAddress = lastMetaReocrdAddress;
                metaBlock.SetByRecordAddress(currentMetaReocrdAddress);
                await metaBlock.RecordVisitor.Read(Database, currentMetaReocrdAddress, buffer =>
                {
                    MetaRecord record = new();
                    record.Read(buffer, 0);
                    //Database.DebugLogger.WriteLine($"ContainsBlockAddress {TypeName} {address} finding {new Json(record.BlockAddresses?.Where(item=>item!=0).ToArray()).ToString(false)}");
                    for (int i = 0; i < record.BlockAddresses!.Length; i++)
                    {
                        if (record.BlockAddresses[i] != 0)
                        {
                            CacheBlockAddresses.Process(() =>
                            {
                                CacheBlockAddresses.Value.Add(record.BlockAddresses[i]);
                            });
                        }
                        if (record.BlockAddresses[i] == 0)
                        {
                            continue;
                        }
                        else if (record.BlockAddresses[i] == address)
                        {
                            result = true;
                            break;
                        }
                        
                    }
                    lastMetaReocrdAddress = record.NextMetaRecordAddress;
                });
                if(result)
                {
                    break;
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.MetaBlockPool.Enqueue(metaBlock);
            Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        }
        return result;
    }

    public async Task RemoveRecord(long recordAddress)
    {
        var statisticalBlock = await Database.Cache.StatisticalBlockPool.Dequeue();
        try
        {
            statisticalBlock.SetByRecordAddress(recordAddress, await GetRecordSize());
            await statisticalBlock.UnuseByAddress(Database, recordAddress);
        }
        catch
        {
            throw;
        }
        finally
        {
            Database.Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        }
    }
}
