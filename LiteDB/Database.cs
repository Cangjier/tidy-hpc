using System.Collections.Concurrent;
using TidyHPC.LiteDB.Arrays;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.Caches;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.LiteDB.Debuggers;
using TidyHPC.LiteDB.Dictionarys;
using TidyHPC.LiteDB.Hashes;
using TidyHPC.LiteDB.HashSets;
using TidyHPC.LiteDB.Metas;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using TidyHPC.Semaphores;

namespace TidyHPC.LiteDB;

/// <summary>
/// Database
/// <para>Header:{IsInitialed:byte,DatabaseSize:long}</para>
/// <para>TypeTable:HashTable//用于映射类型完全限定名->MetaReocrd</para>
/// <para>MetaRecords:MetaBlock//用于记录元数据,其中第0个TypeName为Meta，第1个是HashTable</para>
/// <para></para>
/// </summary>
public class Database
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public Database()
    {
        
    }

    #region Header Config
    internal const long FlagAddress = 0;

    internal const int FlagSize = sizeof(bool);

    internal const long DatabaseSizeAddress = FlagAddress + FlagSize;

    internal const int DatabaseSizeSize = sizeof(long);

    internal const int HeaderSize = FlagSize + DatabaseSizeSize;

    /// <summary>
    /// 数据入口的地址
    /// </summary>
    internal const long EntryAddress = HeaderSize;

    internal const int BlockSize = 1024 * 1024;

    /// <summary>
    /// TypeTable的地址
    /// </summary>
    internal const long InterfaceTableAddress = EntryAddress;

    /// <summary>
    /// 第一个Statictical Block [MetaRecord] 的地址
    /// </summary>
    internal const long NativeMetaBlockAddress = EntryAddress + BlockSize;

    /// <summary>
    /// HashStringSet的地址
    /// </summary>
    internal const long HashStringSetBlockAddress = NativeMetaBlockAddress + BlockSize;

    /// <summary>
    /// 获取Block的地址
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    internal static long GetBlockAddress(long address) => (address - EntryAddress) / BlockSize * BlockSize + EntryAddress;

    #endregion

    internal FileStreamQueue FileStream { get; } = new();

    internal QueueLogger Logger { get; private set; } = null!;

    /// <summary>
    /// TypeName -> MetaAllocater
    /// </summary>
    internal ConcurrentDictionary<string,MetaAllocater> MetaAllocaters { get; } = new();

    /// <summary>
    /// TypeName -> ObjectInterface
    /// </summary>
    internal ConcurrentDictionary<string,ObjectInterfaceRuntime> ObjectInterfaces { get; } = new();

    internal StringHashSet StringHashSet { get; private set; } = null!;

    /// <summary>
    /// 调试器
    /// </summary>
    public Debugger Debuger { get; private set; } = null!;

    #region 信号量
    internal ReaderWriterSemaphorePoolArray<long> SemaphorePoolForAddress { get; } = new(Environment.ProcessorCount * 2, 8);

    internal ReaderWriterSemaphorePool<long> UsedCountSemaphore => SemaphorePoolForAddress[0];

    internal ReaderWriterSemaphorePool<long> StatisticalSemaphore => SemaphorePoolForAddress[1];

    internal ReaderWriterSemaphorePool<long> RecordSemaphore => SemaphorePoolForAddress[2];

    internal ReaderWriterSemaphorePool<long> ArraySemaphore => SemaphorePoolForAddress[3];

    /// <summary>
    /// 申请Block的信号量
    /// </summary>
    internal SemaphoreSlim AllocateBlockSemaphore { get; } = new(1, 1);
    #endregion

    internal Cache Cache { get; private set; } = null!;

    #region Boundary
    private long _DatabaseSize { get; set; } = -1;

    /// <summary>
    /// 表格边界地址
    /// </summary>
    internal long DatabaseSize
    {
        get => _DatabaseSize;
    }

    /// <summary>
    /// 设置表格边界地址
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal async Task SetDatabaseSize(long value)
    {
        if (value > _DatabaseSize)
        {
            _DatabaseSize = value;
            await FileStream.WriteLongAsync(DatabaseSizeAddress, value);
        }
    }
    #endregion

    /// <summary>
    /// Statistical Block [MetaRecord]
    /// </summary>
    internal const string __META_RECORD__ = "__META_RECORD__";

    /// <summary>
    /// Array Block [HashNode]
    /// </summary>
    internal const string __HASH_TABLE__ = "__HASH_TABLE__";

    /// <summary>
    /// Statistical Block [MetaDefineRecord]
    /// </summary>
    internal const string __META_DEFINE_RECORD__ = "__META_DEFINE_RECORD__";

    ///// <summary>
    ///// Statistical Block [HashRecord]
    ///// </summary>
    //internal const string __HASH_RECORD__ = "__HASH_RECORD__";

    /// <summary>
    /// Statistical Block [StringRecord]
    /// </summary>
    internal const string __STRING_RECORD__ = "__STRING_RECORD__";

    ///// <summary>
    ///// Statistical Block [ArrayRecord]
    ///// </summary>
    //internal const string __ARRAY_RECORD__ = "__ARRAY_RECORD__";

    /// <summary>
    /// 启动
    /// </summary>
    public async Task Startup(string databasePath)
    {
        Logger = new(databasePath + ".log");
        FileStream.Open(databasePath, Environment.ProcessorCount);
        StringHashSet = new(this, HashStringSetBlockAddress);
        Debuger = new(this);
        Cache = new(this);

        Logger.WriteLine($"startup {DateTime.Now:O}");

        bool isInitialed = (await FileStream.GetLengthAsync()) == 0 ? false : await FileStream.ReadBooleanAsync(0);
        if (!isInitialed)
        {
            await FileStream.WriteBooleanAsync(0, true);
            await SetDatabaseSize(EntryAddress);
            
            //初始化TypeTable
            var hashTable = await Cache.HashTablePool.Dequeue();
            hashTable.SetAddress(InterfaceTableAddress);
            await hashTable.Initialize(this);
            await SetDatabaseSize(hashTable.BoundarySize);
            Cache.HashTablePool.Enqueue(hashTable);

            //初始化NativeMetaBlock
            var metaBlock = await Cache.MetaBlockPool.Dequeue();
            metaBlock.Set(NativeMetaBlockAddress);
            await metaBlock.Initialize(this);
            await SetDatabaseSize(metaBlock.BoundarySize);
            Cache.MetaBlockPool.Enqueue(metaBlock);

            //初始化StringHashSet
            await StringHashSet.Initialize();
            await SetDatabaseSize(StringHashSet.HashTable.BoundarySize);

            //将 __META_RECORD__ 注册到TypeTable
            await RegisterNativeInterface(__META_RECORD__, MetaRecord.Size, [NativeMetaBlockAddress]);
            //将 __HASH_TABLE__ 注册到TypeTable
            await RegisterNativeInterface(__HASH_TABLE__, 0, [InterfaceTableAddress, HashStringSetBlockAddress]);
            //将 __META_DEFINE_RECORD__ 注册到TypeTable
            await RegisterNativeInterface(__META_DEFINE_RECORD__, MetaDefineRecord.Size, []);
            //将 __HASH_RECORD__ 注册到TypeTable
            await RegisterNativeInterface(HashRecord<Int64Value>.InterfaceName, HashRecord<Int64Value>.Size, []);
            await RegisterNativeInterface(HashRecord<GuidValue>.InterfaceName, HashRecord<GuidValue>.Size, []);
            //将 __STRING_RECORD__ 注册到TypeTable
            await RegisterNativeInterface(__STRING_RECORD__, StringRecord.Size, []);
            //将 __ARRAY_RECORD__ 注册到TypeTable
            await RegisterNativeInterface(ArrayRecord<Int64Value>.InterfaceName, ArrayRecord<Int64Value>.Size, []);
            await RegisterNativeInterface(ArrayRecord<GuidValue>.InterfaceName, ArrayRecord<GuidValue>.Size, []);
            //将 KeyValueRecord系列 注册到TypeTable
            await RegisterNativeInterface(KeyValueRecord<DBReferenceString>.GetFullName(), KeyValueRecord<DBReferenceString>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBGuid>.GetFullName(), KeyValueRecord<DBGuid>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_8>>.GetFullName(), KeyValueRecord<DBString<Interger_8>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_16>>.GetFullName(), KeyValueRecord<DBString<Interger_16>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_32>>.GetFullName(), KeyValueRecord<DBString<Interger_32>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_64>>.GetFullName(), KeyValueRecord<DBString<Interger_64>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_128>>.GetFullName(), KeyValueRecord<DBString<Interger_128>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBString<Interger_256>>.GetFullName(), KeyValueRecord<DBString<Interger_256>>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBMD5>.GetFullName(), KeyValueRecord<DBMD5>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBInt32>.GetFullName(), KeyValueRecord<DBInt32>.GetSize(), []);
            await RegisterNativeInterface(KeyValueRecord<DBInt64>.GetFullName(), KeyValueRecord<DBInt64>.GetSize(), []);
        }
        else
        {
            _DatabaseSize = await FileStream.ReadLongAsync(DatabaseSizeAddress);
        }
    }

    private SemaphorePool<string> GetMetaAllocaterSemaphorePool { get; } = new();

    /// <summary>
    /// 获取MetaAllocater
    /// </summary>
    /// <returns></returns>
    internal async Task<MetaAllocater> GetMetaAllocater(string interfaceName)
    {
        await GetMetaAllocaterSemaphorePool.WaitAsync(interfaceName);
        try
        {
            if (MetaAllocaters.TryGetValue(interfaceName, out MetaAllocater? metaAllocater))
            {
                return metaAllocater;
            }
            var hashTable = await Cache.HashTablePool.Dequeue();
            var metaBlock = await Cache.MetaBlockPool.Dequeue();
            var typeNameBytes = Util.UTF8.GetBytes(interfaceName);
            var typeNameHashCode = await HashService.GetHashCode(typeNameBytes);
            hashTable.SetAddress(InterfaceTableAddress);
            var getResult = await hashTable.Get(this, typeNameHashCode, async objectAddress =>
            {
                bool isEquals = false;
                metaBlock.SetByRecordAddress(objectAddress);
                await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
                {
                    MetaRecord metaRecord = new();
                    metaRecord.Read(buffer, 0);
                    isEquals = interfaceName == metaRecord.TypeName;
                });
                return isEquals;
            });
            Cache.HashTablePool.Enqueue(hashTable);
            Cache.MetaBlockPool.Enqueue(metaBlock);
            if (getResult.Success)
            {
                metaAllocater = new(this, interfaceName, getResult.Value);
                MetaAllocaters.TryAdd(interfaceName, metaAllocater);
                return metaAllocater;
            }
            else
            {
                throw new Exception($"Unkonwn interface `{interfaceName}`");
                //throw new Exception($"get MetaAllocater failed, hashCode={typeNameHashCode}");
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            await GetMetaAllocaterSemaphorePool.ReleaseAsync(interfaceName);
        }
    }

    /// <summary>
    /// 获取ObjectInterface
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <returns></returns>
    internal async Task<ObjectInterfaceRuntime?> GetObjectInterface(string interfaceName)
    {
        if(ObjectInterfaces.TryGetValue(interfaceName,out ObjectInterfaceRuntime? objectInterface))
        {
            return objectInterface;
        }
        var metaRecord = await GetMetaRecord(interfaceName);
        if(metaRecord == null)
        {
            throw new Exception($"Interface `{interfaceName}` not registered");
        }
        if (metaRecord.Value.DefineRecordAddress == 0)
        {
            return null;
        }
        var defineRecord = await GetRecord<MetaDefineRecord>(metaRecord.Value.DefineRecordAddress, MetaDefineRecord.Size);
        ObjectInterfaceRuntime result = new();
        await result.Parse(this, metaRecord.Value, defineRecord);
        ObjectInterfaces.TryAdd(interfaceName, result);
        return result;
    }

    internal async Task<MetaRecord?> GetMetaRecord(string interfaceName)
    {
        MetaRecord result = new();
        var hashTable = await Cache.HashTablePool.Dequeue();
        var metaBlock = await Cache.MetaBlockPool.Dequeue();
        var typeNameBytes = Util.UTF8.GetBytes(interfaceName);
        var typeNameHashCode = await HashService.GetHashCode(typeNameBytes);
        hashTable.SetAddress(InterfaceTableAddress);
        var getResult = await hashTable.Get(this, typeNameHashCode, async objectAddress =>
        {
            bool isEquals = false;
            metaBlock.SetByRecordAddress(objectAddress);
            await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
            {
                result = new();
                result.Read(buffer, 0);
                isEquals = result.TypeName == interfaceName;
            });
            return isEquals;
        });
        Cache.HashTablePool.Enqueue(hashTable);
        Cache.MetaBlockPool.Enqueue(metaBlock);
        if (getResult.Success)
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 是否包含类型
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <returns></returns>
    public async Task<bool> ContainsInterface(string interfaceName)
    {
        if(MetaAllocaters.ContainsKey(interfaceName))
        {
            return true;
        }
        var hashTable = await Cache.HashTablePool.Dequeue();
        var metaBlock = await Cache.MetaBlockPool.Dequeue();
        var typeNameBytes = Util.UTF8.GetBytes(interfaceName);
        hashTable.SetAddress(InterfaceTableAddress);
        var getResult = await hashTable.Get(this, await HashService.GetHashCode(typeNameBytes), async objectAddress =>
        {
            bool isEquals = false;
            metaBlock.SetByRecordAddress(objectAddress);
            await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
            {
                MetaRecord metaRecord = new();
                metaRecord.Read(buffer, 0);
                isEquals = interfaceName == metaRecord.TypeName;
            });
            return isEquals;
        });
        Cache.HashTablePool.Enqueue(hashTable);
        Cache.MetaBlockPool.Enqueue(metaBlock);
        return getResult.Success;
    }

    /// <summary>
    /// 获取所有的类型名称
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string[]> GetInterfaceNames()
    {
        List<string> result = new();
        var metaRecord = await GetMetaRecord(__META_RECORD__);
        if(metaRecord == null)
        {
            throw new Exception("MetaRecord not found");
        }
        var metaBlock = await Cache.MetaBlockPool.Dequeue();
        var statisticalBlock = await Cache.StatisticalBlockPool.Dequeue();
        var lastMetaRecordAddress = metaRecord.Value.FirstMetaRecordAddress;
        while (true)
        {
            MetaRecord record = new();
            metaBlock.SetByRecordAddress(lastMetaRecordAddress);
            await metaBlock.RecordVisitor.Read(this, lastMetaRecordAddress, buffer =>
            {
                record.Read(buffer, 0);
            });
            var blocks = record.BlockAddresses;
            for (int i = 0; i < blocks!.Length; i++)
            {
                if (blocks[i] == 0)
                {
                    continue ;
                }
                statisticalBlock.Set(blocks[i], MetaRecord.Size);
                var indexs =await statisticalBlock.GetUsedIndexs(this);
                for (int j = 0; j < indexs.Length; j++)
                {
                    await statisticalBlock.RecordVisitor.ReadByIndex(this, indexs[j], (address, buffer) =>
                    {
                        MetaRecord itemMetaRecord = new();
                        itemMetaRecord.Read(buffer, 0);
                        if (itemMetaRecord.FirstMetaRecordAddress == address&& itemMetaRecord.TypeName!=null)
                        {
                            result.Add(itemMetaRecord.TypeName);
                        }
                    });
                }
            }
            if (record.NextMetaRecordAddress == 0)
            {
                break;
            }
            lastMetaRecordAddress = record.NextMetaRecordAddress;
        }
        Cache.MetaBlockPool.Enqueue(metaBlock);
        Cache.StatisticalBlockPool.Enqueue(statisticalBlock);
        return result.ToArray();
    }

    /// <summary>
    /// 分配Block
    /// </summary>
    /// <returns></returns>
    internal async Task<long> AllocateBlock()
    {
        await AllocateBlockSemaphore.WaitAsync();
        var address = DatabaseSize;
        _ = SetDatabaseSize(address + BlockSize);
        AllocateBlockSemaphore.Release();
        //初始化Block
        var buffer = await Cache.DequeueBuffer(BlockSize);
        await FileStream.WriteAsync(address, buffer, 0, BlockSize);
        Cache.EnqueueBuffer(buffer);
        return address;
    }

    /// <summary>
    /// 申请一个记录
    /// </summary>
    /// <returns></returns>
    /// <param name="interfaceName"></param>
    internal async Task<long> AllocateRecord(string interfaceName)
    {
        if (interfaceName == __HASH_TABLE__)
        {
            throw new Exception("Can't allocate HashTable");
        }
        var metaAllocater = await GetMetaAllocater(interfaceName);
        return await metaAllocater.AllocateRecord();
    }

    internal async Task<long> AllocateHashTable()
    {
        var metaAllocater = await GetMetaAllocater(__HASH_TABLE__);
        return await metaAllocater.AllocateHashTable();
    }

    /// <summary>
    /// 申请一个Hash记录
    /// </summary>
    /// <returns></returns>
    internal async Task<long> AllocateHashRecord<TValue>()
        where TValue : struct, IValue<TValue>
    {
        return await AllocateRecord(HashRecord<TValue>.InterfaceName);
    }

    /// <summary>
    /// 申请一个新的数组记录
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    internal async Task<long> AllocateArrayRecord<TValue>()
        where TValue : struct, IValue<TValue>
    {
        return await AllocateRecord(ArrayRecord<TValue>.InterfaceName);
    }

    /// <summary>
    /// 注册一个类型
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="recordSie"></param>
    /// <param name="nativeBlockAddresses"></param>
    /// <returns></returns>
    internal async Task RegisterNativeInterface(string interfaceName, int recordSie,long[] nativeBlockAddresses)
    {
        var hashTable = await Cache.HashTablePool.Dequeue();
        hashTable.SetAddress(InterfaceTableAddress);
        var metaBlock = await Cache.MetaBlockPool.Dequeue();
        metaBlock.Set(NativeMetaBlockAddress);
        var firstMetaRecordAddress = (await metaBlock.AllocateRecord(this)).Value;
        var typeNameBytes = Util.UTF8.GetBytes(interfaceName);
        var hashCode = await HashService.GetHashCode(typeNameBytes);

        await hashTable.Update(this, hashCode,
            async objectAddress =>
            {
                bool isEquals = false;
                metaBlock.SetByRecordAddress(objectAddress);
                await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
                {
                    MetaRecord metaRecord = new();
                    metaRecord.Read(buffer, 0);
                    isEquals = metaRecord.TypeName == interfaceName;
                });
                return isEquals;
            },
            async () =>
            {
                //新建状态
                var record = firstMetaRecordAddress;
                metaBlock.SetByRecordAddress(record);
                await metaBlock.RecordVisitor.Update(this, record, buffer =>
                {
                    MetaRecord metaRecord = new();
                    metaRecord.Read(buffer, 0);
                    metaRecord.FirstMetaRecordAddress = firstMetaRecordAddress;
                    metaRecord.TypeName = interfaceName;
                    metaRecord.RecordSize = recordSie;
                    for (int i = 0; i < nativeBlockAddresses.Length; i++)
                    {
                        metaRecord.BlockAddresses![i] = nativeBlockAddresses[i];
                    }
                    metaRecord.Write(buffer, 0);
                    return true;
                });
                return record;
            },
            async value =>
            {
                await Task.CompletedTask;
                return value;
            });
        bool registered = false;
        await hashTable.Get(this, hashCode, async objectAddress =>
        {
            metaBlock.SetByRecordAddress(objectAddress);
            await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
            {
                MetaRecord metaRecord = new();
                metaRecord.Read(buffer, 0);
                if(interfaceName == metaRecord.TypeName)
                {
                    registered = true;
                }
            });
            return true;
        });
        if(registered == false)
        {
            throw new Exception("RegisterNativeType failed");
        }
        Cache.HashTablePool.Enqueue(hashTable);
        Cache.MetaBlockPool.Enqueue(metaBlock);
    }

    /// <summary>
    /// 注册一个类型
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="recordSie"></param>
    /// <param name="metaDefineRecordAddress"></param>
    /// <returns></returns>
    internal async Task<long> RegisterUserInterface(string interfaceName,int recordSie,long metaDefineRecordAddress)
    {
        long result = 0;
        var hashTable = await Cache.HashTablePool.Dequeue();
        hashTable.SetAddress(InterfaceTableAddress);
        var metaBlock = await Cache.MetaBlockPool.Dequeue();
        var typeNameBytes = Util.UTF8.GetBytes(interfaceName);
        var hashCode = await HashService.GetHashCode(typeNameBytes);

        await hashTable.Update(this, hashCode,
            async objectAddress =>
            {
                bool isEquals = false;
                metaBlock.SetByRecordAddress(objectAddress);
                await metaBlock.RecordVisitor.Read(this, objectAddress, buffer =>
                {
                    MetaRecord metaRecord = new();
                    metaRecord.Read(buffer, 0);
                    isEquals = metaRecord.TypeName == interfaceName;
                });
                return isEquals;
            },
            async () =>
            {
                //新建状态
                var record = await AllocateRecord(__META_RECORD__);
                metaBlock.SetByRecordAddress(record);
                await metaBlock.RecordVisitor.Update(this, record, buffer =>
                {
                    MetaRecord metaRecord = new();
                    metaRecord.Read(buffer, 0);
                    metaRecord.TypeName = interfaceName;
                    metaRecord.DefineRecordAddress = metaDefineRecordAddress;
                    metaRecord.RecordSize = recordSie;
                    metaRecord.NextMetaRecordAddress = 0;
                    metaRecord.FirstMetaRecordAddress = record;
                    metaRecord.Write(buffer, 0);
                    return true;
                });
                result= record;
                return record;
            },
            async value =>
            {
                result = value;
                await Task.CompletedTask;
                return value;
            });
        Cache.HashTablePool.Enqueue(hashTable);
        Cache.MetaBlockPool.Enqueue(metaBlock);

        return result;
    }

    /// <summary>
    /// 申请一个新的元数据定义记录
    /// </summary>
    /// <param name="objectInterface"></param>
    /// <param name="fieldMapAddresses"></param>
    /// <returns></returns>
    internal async Task<long> AllocateDefineRecord(ObjectInterface objectInterface,long[] fieldMapAddresses)
    {
        var defineRecordAddress = await AllocateRecord(__META_DEFINE_RECORD__);
        long[] refString = new long[objectInterface.Fields.Count];
        byte[] fieldTypes = new byte[objectInterface.Fields.Count];
        byte[] fieldMapTypes = new byte[objectInterface.Fields.Count];
        int[] fieldArrayLength = new int[objectInterface.Fields.Count];
        for (int i = 0; i < objectInterface.Fields.Count; i++)
        {
            refString[i] = await StringHashSet.New(objectInterface.Fields[i].Name);
            fieldTypes[i] = (byte)objectInterface.Fields[i].Type;
            fieldMapTypes[i] = (byte)objectInterface.Fields[i].MapType;
            fieldArrayLength[i] = objectInterface.Fields[i].ArrayLength;
        }
        await EditRecord<MetaDefineRecord>(defineRecordAddress, MetaDefineRecord.Size, record =>
        {
            record.FieldCount = objectInterface.Fields.Count;
            refString.CopyTo(record.FieldNames, 0);
            fieldTypes.CopyTo(record.FieldTypes, 0);
            fieldMapTypes.CopyTo(record.FieldMapTypes, 0);
            fieldMapAddresses.CopyTo(record.FieldMapAddresses, 0);
            fieldArrayLength.CopyTo(record.FieldArrayLengths, 0);
            return record;
        });
        return defineRecordAddress;
    }

    /// <summary>
    /// 编辑记录
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="recordAddress"></param>
    /// <param name="recordSize"></param>
    /// <param name="onEdit"></param>
    /// <returns></returns>
    internal async Task EditRecord<T>(long recordAddress, int recordSize, Func<T, T> onEdit)
        where T : IRecord, new()
    {
        var block = await Cache.StatisticalBlockPool.Dequeue();
        block.SetByRecordAddress(recordAddress, recordSize);
        await block.RecordVisitor.Update(this,recordAddress, buffer =>
        {
            T record = new();
            record.Read(buffer, 0);
            record = onEdit(record);
            record.Write(buffer, 0);
            return true;
        });
        Cache.StatisticalBlockPool.Enqueue(block);
    }

    internal async Task EditRecord(long recordAddress, int recordSize, Action<byte[]> onEdit)
    {
        var block = await Cache.StatisticalBlockPool.Dequeue();
        block.SetByRecordAddress(recordAddress, recordSize);
        await block.RecordVisitor.Update(this, recordAddress, buffer =>
        {
            onEdit(buffer);
            return true;
        });
        Cache.StatisticalBlockPool.Enqueue(block);
    }

    internal async Task EditRecord(long recordAddress, int recordSize, Func<byte[],Task> onEdit)
    {
        var block = await Cache.StatisticalBlockPool.Dequeue();
        block.SetByRecordAddress(recordAddress, recordSize);
        await block.RecordVisitor.Update(this, recordAddress, async buffer =>
        {
            await onEdit(buffer);
            return true;
        });
        Cache.StatisticalBlockPool.Enqueue(block);
    }

    internal async Task<T> GetRecord<T>(long recordAddress,int recordSize)
        where T : IRecord, new()
    {
        T result = new();
        var block = await Cache.StatisticalBlockPool.Dequeue();
        block.SetByRecordAddress(recordAddress, recordSize);
        await block.RecordVisitor.Read(this, recordAddress, buffer =>
        {
            result.Read(buffer, 0);
        });
        Cache.StatisticalBlockPool.Enqueue(block);
        return result;
    }

    /// <summary>
    /// 注册一个类型
    /// </summary>
    /// <param name="objectInterface"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task RegisterInterface(ObjectInterface objectInterface)
    {
        Logger.WriteLine($"register interface {objectInterface.ToString(false)}");
        //首先，确认是否已经注册
        if (await ContainsInterface(objectInterface.FullName))
        {
            throw new Exception("Type has been registered");
        }
        //其次，根据字段映射类型，注册相应的HashTable
        long[] mapAddresses = new long[objectInterface.Fields.Count];
        for (int i = 0; i < objectInterface.Fields.Count; i++)
        {
            var mapType = objectInterface.Fields[i].MapType;
            if (mapType == FieldMapType.Master ||
                mapType == FieldMapType.Index ||
                mapType == FieldMapType.IndexArray ||
                mapType == FieldMapType.IndexHashSet)
            {
                mapAddresses[i] = await AllocateHashTable();
            }
            else if(mapType== FieldMapType.IndexSmallHashSet)
            {
                mapAddresses[i] = await AllocateHashRecord<Int64Value>();
            }
        }
        //其次，申请一个MetaDefineRecord
        var defineRecordAddress = await AllocateDefineRecord(objectInterface, mapAddresses);
        //其次，注册一个新的类型
        await RegisterUserInterface(objectInterface.FullName, objectInterface.GetSize(), defineRecordAddress);
    }

    /// <summary>
    /// 获取所有记录
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="onRecord"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task All(string interfaceName,Action<Json> onRecord)
    {
        Logger.WriteLine($"all {interfaceName}");
        var metaAllocater = await GetMetaAllocater(interfaceName);
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        var blockAddresses =await metaAllocater.GetBlockAddresses();
        var block = await Cache.StatisticalBlockPool.Dequeue();
        foreach(var blockAddress in blockAddresses)
        {
            block.Set(blockAddress, objectInterface.GetSize());
            var indexs = await block.GetUsedIndexs(this);
            for (int i = 0; i < indexs.Length; i++)
            {
                RecordRuntime recordRuntime = objectInterface.NewRecordRuntime();
                await block.RecordVisitor.ReadByIndex(this, indexs[i], (address, buffer) =>
                {
                    recordRuntime.DeserializeFromBuffer(buffer, 0);
                });
                onRecord(await recordRuntime.SerializeToJson(this));
            }
        }
        Cache.StatisticalBlockPool.Enqueue(block);
    }

    /// <summary>
    /// 根据主键查找记录地址
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="master"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal async Task<long> GetRecordAddressByMaster(string interfaceName,Guid master)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        long mappingHashTableAddress = 0;
        FieldType masterType = FieldType.Guid;
        int index = -1;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if (field.MapType == FieldMapType.Master)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                masterType = field.Type;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            throw new Exception("Master field not found");
        }
        if (masterType == FieldType.Guid)
        {
            return await Cache.DictionaryVisitor.GetGuid(mappingHashTableAddress, master);
        }
        else
        {
            throw new Exception("Unsupported type");
        }
    }

    /// <summary>
    /// 根据索引查找记录地址
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="indexName"></param>
    /// <param name="indexValue"></param>
    /// <returns></returns>
    internal async Task<long> GetRecordAddressByIndex(string interfaceName, string indexName, object indexValue)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        long mappingHashTableAddress = 0;
        Field indexField = new();
        int index = -1;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if ((field.MapType == FieldMapType.Index || field.MapType == FieldMapType.Master) && field.Name == indexName)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                indexField = field;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            throw new Exception("Index field not found");
        }
        if(indexField.Type == FieldType.ReferneceString)
        {
            indexValue = await StringHashSet.Borrow((string)indexValue);
        }
        else if(indexField.Type == FieldType.MD5)
        {
            indexValue = Util.HexToBytes((string)indexValue);
        }
        return await Cache.DictionaryVisitor.Get(indexField, mappingHashTableAddress, indexValue);
    }

    /// <summary>
    /// 根据索引获取记录地址集合
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="indexName"></param>
    /// <param name="indexValue"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<long[]> GetRecordAddressesByIndexArray(string interfaceName,string indexName,object indexValue)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        long mappingHashTableAddress = 0;
        Field indexField = new();
        int index = -1;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if (field.MapType == FieldMapType.IndexArray && field.Name == indexName)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                indexField = field;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            throw new Exception("Index field not found");
        }
        if (indexField.Type == FieldType.ReferneceString)
        {
            indexValue = await StringHashSet.Borrow((string)indexValue);
        }
        else if (indexField.Type == FieldType.MD5)
        {
            indexValue = Util.HexToBytes((string)indexValue);
        }
        List<long> values = [];
        await Cache.DictionaryVisitor.GetArray(indexField, mappingHashTableAddress, indexValue, values.Add);
        return [.. values];
    }

    /// <summary>
    /// 根据索引获取记录地址集合
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="indexName"></param>
    /// <param name="indexValue"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<long[]> GetRecordAddressesByIndexHashSet(string interfaceName,string indexName,object indexValue)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        long mappingHashTableAddress = 0;
        Field indexField = new();
        int index = -1;
        FieldMapType mapType = FieldMapType.IndexHashSet;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if ((field.MapType == FieldMapType.IndexHashSet||
                field.MapType == FieldMapType.IndexSmallHashSet) && field.Name == indexName)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                indexField = field;
                mapType = field.MapType;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            throw new Exception("Index field not found");
        }
        if (indexField.Type == FieldType.ReferneceString)
        {
            indexValue = await StringHashSet.Borrow((string)indexValue);
        }
        else if (indexField.Type == FieldType.MD5)
        {
            indexValue = Util.HexToBytes((string)indexValue);
        }
        List<long> values = [];
        if (mapType == FieldMapType.IndexHashSet)
        {
            await Cache.DictionaryVisitor.GetHashSet(indexField, mappingHashTableAddress, indexValue, values.Add);
        }
        else if (mapType == FieldMapType.IndexSmallHashSet)
        {
            await Cache.DictionaryVisitor.GetSmallHashSet(indexField, mappingHashTableAddress, indexValue, values.Add);
        }
        return [.. values];
    }

    /// <summary>
    /// 判断是否包含索引
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="indexName"></param>
    /// <param name="indexValue"></param>
    /// <returns></returns>
    public async Task<bool> ContainsByIndex(string interfaceName,string indexName,object indexValue)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            return false;
        }
        long mappingHashTableAddress = 0;
        Field indexField = new();
        int index = -1;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if ((field.MapType == FieldMapType.Index||
                field.MapType == FieldMapType.Master) && field.Name == indexName)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                indexField = field;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            return false;
        }
        if (indexField.Type == FieldType.ReferneceString)
        {
            indexValue = await StringHashSet.Borrow((string)indexValue);
        }
        else if (indexField.Type == FieldType.MD5)
        {
            indexValue = Util.HexToBytes((string)indexValue);
        }
        return await Cache.DictionaryVisitor.ContainsKey(indexField, mappingHashTableAddress, indexValue);
    }

    /// <summary>
    /// 判断是否包含主键
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="master"></param>
    /// <returns></returns>
    public async Task<bool> ContainsByMaster(string interfaceName,Guid master)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            return false;
        }
        long mappingHashTableAddress = 0;
        FieldType masterType = FieldType.Guid;
        int index = -1;
        foreach (var field in objectInterface.Fields)
        {
            index++;
            if (field.MapType == FieldMapType.Master)
            {
                mappingHashTableAddress = objectInterface.MappingAddress![index];
                masterType = field.Type;
                break;
            }
        }
        if (mappingHashTableAddress == 0)
        {
            return false;
        }
        if (masterType == FieldType.Guid)
        {
            return await Cache.DictionaryVisitor.ContainsGuid(mappingHashTableAddress, master);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Find the record by master key
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="master"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Json> FindByMaster(string interfaceName,Guid master)
    {
        Logger.WriteLine($"query master {interfaceName} {master}");
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        var recordAddress = await GetRecordAddressByMaster(interfaceName, master);
        using var recordRuntime = await objectInterface.DeserializeFromAddress(this, recordAddress);
        return await recordRuntime.SerializeToJson(this);
    }

    /// <summary>
    /// 根据索引查找记录
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="indexName"></param>
    /// <param name="indexValue"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Json> FindByIndex(string interfaceName,string indexName,object indexValue)
    {
        try
        {
            Logger.WriteLine($"query index {interfaceName} {indexName} {indexValue}");
            var objectInterface = await GetObjectInterface(interfaceName);
            if (objectInterface == null)
            {
                throw new Exception("ObjectInterface not found");
            }
            var recordAddress = await GetRecordAddressByIndex(interfaceName, indexName, indexValue);
            using var recordRuntime = await objectInterface.DeserializeFromAddress(this, recordAddress);
            return await recordRuntime.SerializeToJson(this);
        }
        catch(Exception e)
        {
            Loggers.Logger.Error(e);
            throw;
        }
    }

    /// <summary>
    /// Find the record by address
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Json> FindByAddress(string interfaceName,long address)
    {
        Logger.WriteLine($"query address {interfaceName} {address}");
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        using var recordRuntime = await objectInterface.DeserializeFromAddress(this, address);
        return await recordRuntime.SerializeToJson(this);
    }

    /// <summary>
    /// 根据记录地址获取MetaAllocater
    /// </summary>
    /// <param name="recordAddress"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal async Task<MetaAllocater?> GetMetaAllocaterByRecordAddress(long recordAddress)
    {
        MetaAllocater? metaAllocater = null;
        var tempTypeNames = Cache.CacheInterfaceNames.Value.ToArray();
        foreach (var typeName in tempTypeNames)
        {
            var item = await GetMetaAllocater(typeName);
            if (await item.ContainsBlockAddress(recordAddress))
            {
                metaAllocater = item;
                break;
            }
        }
        if (metaAllocater == null)
        {
            var allTypeNames = await GetInterfaceNames();
            var otherTypeNames = allTypeNames.Except(tempTypeNames).ToArray();
            foreach (var typeName in otherTypeNames)
            {
                var item = await GetMetaAllocater(typeName);
                if (await item.ContainsBlockAddress(recordAddress))
                {
                    metaAllocater = item;
                    break;
                }
            }
        }
        if (metaAllocater == null)
        {
            throw new Exception("MetaAllocater not found");
        }
        return metaAllocater;
    }

    /// <summary>
    /// Find the record by address
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<Json> FindByAddress(long address)
    {
        MetaAllocater? metaAllocater = await GetMetaAllocaterByRecordAddress(address);
        if(metaAllocater == null)
        {
            throw new Exception("MetaAllocater not found");
        }
        var objectInterface = await GetObjectInterface(metaAllocater.TypeName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        using var recordRuntime = await objectInterface.DeserializeFromAddress(this, address);
        return await recordRuntime.SerializeToJson(this);
    }

    /// <summary>
    /// 插入一个记录
    /// </summary>
    /// <returns></returns>
    public async Task<RecordIndex> Insert(string interfaceName, Json document)
    {
        Logger.WriteLine($"insert {interfaceName} {document.ToString(false)}");
        var metaAllocater = await GetMetaAllocater(interfaceName);
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        //首选对记录进行校验
        objectInterface.Validate(document);
        //申请记录
        var recordAddress = await metaAllocater.AllocateRecord();
        //编辑记录
        RecordRuntime recordRuntime = objectInterface.NewRecordRuntime();
        recordRuntime.Success = false;
        await recordRuntime.DeserializeFromNewJson(this, document, false);
        await EditRecord(recordAddress, objectInterface.GetSize(), record =>
        {
            recordRuntime.SerializeToBuffer(record, 0);
            recordRuntime.Success = true;
        });
        if (recordRuntime.Success == false) throw new Exception("Serialization failed");
        //向HashTable中添加索引
        for (int i = 0; i < objectInterface.Fields.Count; i++)
        {
            var field = objectInterface.Fields[i];
            var recordField = recordRuntime.Fields![i];
            var mapType = field.MapType;
            var type = field.Type;
            if (mapType == FieldMapType.Master ||
                mapType == FieldMapType.Index)
            {
                await Cache.DictionaryVisitor.Set(
                    field, objectInterface.MappingAddress![i], recordField.Value, recordAddress);
            }
            else if (mapType == FieldMapType.IndexArray)
            {
                await Cache.DictionaryVisitor.AddToArray(
                    field, objectInterface.MappingAddress![i], recordField.Value, recordAddress);
            }
            else if (mapType == FieldMapType.IndexHashSet)
            {
                await Cache.DictionaryVisitor.AddToHashSet(
                    field, objectInterface.MappingAddress![i], recordField.Value, recordAddress);
            }
            else if(mapType == FieldMapType.IndexSmallHashSet)
            {
                await Cache.DictionaryVisitor.AddToSmallHashSet(
                    field, objectInterface.MappingAddress![i], recordField.Value, recordAddress);
            }
        }
        //返回索引
        return new RecordIndex()
        {
            Address = recordAddress,
            Master = recordRuntime.Master
        };
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="recordAddress"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Update(string interfaceName,long recordAddress,Json document)
    {
        Logger.WriteLine($"update {interfaceName} {document.ToString(false)}");
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        objectInterface.Validate(document);
        var oldRecordRuntime = await objectInterface.DeserializeFromAddress(this, recordAddress);
        var newRecordRuntime = objectInterface.NewRecordRuntime();
        await newRecordRuntime.DeserializeFromNewJson(this, document,true);
        //变更引用
        for (int i = 0; i < objectInterface.Fields.Count; i++)
        {
            var field = objectInterface.Fields[i];
            var oldRecordField = oldRecordRuntime.Fields![i];
            var newRecordField = newRecordRuntime.Fields![i];
            var mapType = field.MapType;
            var type = field.Type;
            if (oldRecordField.Value.Equals(newRecordField.Value)==false)
            {
                if (mapType == FieldMapType.Master ||
                                mapType == FieldMapType.Index)
                {
                    await Cache.DictionaryVisitor.RemoveKey(
                            field, objectInterface.MappingAddress![i], oldRecordField.Value);
                    await Cache.DictionaryVisitor.Set(
                    field, objectInterface.MappingAddress![i], newRecordField.Value, recordAddress);
                }
                else if (mapType == FieldMapType.IndexArray)
                {
                    await Cache.DictionaryVisitor.RemoveFromArray(
                        field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
                    await Cache.DictionaryVisitor.AddToArray(
                        field, objectInterface.MappingAddress![i], newRecordField.Value, recordAddress);
                }
                else if (mapType == FieldMapType.IndexHashSet)
                {
                    await Cache.DictionaryVisitor.RemoveFromHashSet(
                        field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
                    await Cache.DictionaryVisitor.AddToHashSet(
                        field, objectInterface.MappingAddress![i], newRecordField.Value, recordAddress);
                }
                else if(mapType == FieldMapType.IndexSmallHashSet)
                {
                    await Cache.DictionaryVisitor.RemoveFromSmallHashSet(
                        field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
                    await Cache.DictionaryVisitor.AddToSmallHashSet(
                        field, objectInterface.MappingAddress![i], newRecordField.Value, recordAddress);
                }

                if(type == FieldType.ReferneceString)
                {
                    await oldRecordField.ReleaseReference(this);
                    await newRecordField.IncreaseReference(this);
                }
            }
        }
        await EditRecord(recordAddress, objectInterface.GetSize(), record =>
        {
            newRecordRuntime.SerializeToBuffer(record, 0);
        });
    }

    /// <summary>
    /// 根据Master更新数据
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public async Task UpdateByMaster(string interfaceName,Json document)
    {
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        objectInterface.Validate(document);
        var master = objectInterface.GetMasterByJsonDocument(document);
        var recordAddress = await GetRecordAddressByMaster(interfaceName, master);
        await Update(interfaceName, recordAddress, document);
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="recordAddress"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Delete(string interfaceName,long recordAddress)
    {
        Logger.WriteLine($"delete address {interfaceName} {recordAddress}");
        var objectInterface = await GetObjectInterface(interfaceName);
        var metaAllocater = await GetMetaAllocater(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        var oldRecordRuntime = await objectInterface.DeserializeFromAddress(this, recordAddress);
        for (int i = 0; i < objectInterface.Fields.Count; i++)
        {
            var field = objectInterface.Fields[i];
            var oldRecordField = oldRecordRuntime.Fields![i];
            var mapType = field.MapType;
            var type = field.Type;
            if (mapType == FieldMapType.Master ||
                mapType == FieldMapType.Index)
            {
                await Cache.DictionaryVisitor.RemoveKey(
                        field, objectInterface.MappingAddress![i], oldRecordField.Value);
            }
            else if (mapType == FieldMapType.IndexArray)
            {
                await Cache.DictionaryVisitor.RemoveFromArray(
                    field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
            }
            else if (mapType == FieldMapType.IndexHashSet)
            {
                await Cache.DictionaryVisitor.RemoveFromHashSet(
                    field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
            }
            else if(mapType == FieldMapType.IndexSmallHashSet)
            {
                await Cache.DictionaryVisitor.RemoveFromSmallHashSet(
                    field, objectInterface.MappingAddress![i], oldRecordField.Value, recordAddress);
            }
            if (type == FieldType.ReferneceString)
            {
                await oldRecordField.ReleaseReference(this);
            }
        }
        await metaAllocater.RemoveRecord(recordAddress);
    }

    /// <summary>
    /// 根据Master删除数据
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <param name="master"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task DeleteByMaster(string interfaceName,Guid master)
    {
        Logger.WriteLine($"delete master {interfaceName} {master}");
        var objectInterface = await GetObjectInterface(interfaceName);
        if (objectInterface == null)
        {
            throw new Exception("ObjectInterface not found");
        }
        var recordAddress = await GetRecordAddressByMaster(interfaceName, master);
        await Delete(interfaceName, recordAddress);
    }

    /// <summary>
    /// 根据日志恢复数据
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public async Task RestoreByLogger(string line)
    {
        var items = line.Split(' ');
        var cmd = items[0];
        if (cmd == "register")
        {
            var registerType = items[1];
            if (registerType == "interface")
            {
                var interfaceJson = line.Substring(items[0].Length + items[1].Length + 2);
                var objectInterface = new ObjectInterface();
                objectInterface.DeserializeFromJson(Json.Parse(interfaceJson));
                await RegisterInterface(objectInterface);
            }
        }
        else if (cmd == "insert")
        {
            var interfaceName = items[1];
            var json = line.Substring(items[0].Length + items[1].Length + 2);
            await Insert(interfaceName, Json.Parse(json));
        }
        else if (cmd == "update")
        {
            var updateType = items[1];
            if(updateType == "address")
            {
                var interfaceName = items[2];
                var address = long.Parse(items[3]);
                var json = line.Substring(items[0].Length + items[1].Length + items[2].Length + items[3].Length + 4);
                await Update(interfaceName, address, Json.Parse(json));
            }
            else if(updateType == "master")
            {
                var interfaceName = items[2];
                var json = line.Substring(items[0].Length + items[1].Length + items[2].Length + 3);
                await UpdateByMaster(interfaceName, Json.Parse(json));
            }
        }
        else if(cmd=="delete")
        {
            var deleteType = items[1];
            if(deleteType == "address")
            {
                var interfaceName = items[2];
                var address = long.Parse(items[3]);
                await Delete(interfaceName, address);
            }
            else if(deleteType == "master")
            {
                var interfaceName = items[2];
                var master = Guid.Parse(items[3]);
                await DeleteByMaster(interfaceName, master);
            }
        }
        else if (cmd == "query")
        {
            var queryType = items[1];
            if (queryType == "master")
            {
                var interfaceName = items[2];
                var master = Guid.Parse(items[3]);
                await FindByMaster(interfaceName, master);
            }
            else if (queryType == "index")
            {
                var interfaceName = items[2];
                var indexName = items[3];
                var indexValue = items[4];
                await FindByIndex(interfaceName, indexName, indexValue);
            }
            else if (queryType == "address")
            {
                var address = long.Parse(items[2]);
                await FindByAddress(address);
            }
        }
    }
}
