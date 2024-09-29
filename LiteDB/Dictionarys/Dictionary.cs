using TidyHPC.Common;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.LiteDB.Hashes;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.Loggers;

namespace TidyHPC.LiteDB.Dictionarys;

internal interface IDictionary
{
    internal void SetHashTable(long address);

    internal Task<bool> TryGet(object key, Ref<long> outValue);

    internal Task<bool> ContainsKey(object key);

    internal Task Set(object key, long value);

    internal Task RemoveKey(object key);
}


internal class Dictionary<TKey> : IDictionary
    where TKey : DBType, new()
{
    internal HashTable<Int64Value> HashTable { get; } = new();

    internal Database Database { get; set; } = null!;

    internal void SetHashTable(long address)
    {
        HashTable.SetAddress(address);
    }

    void IDictionary.SetHashTable(long address)
    {
        SetHashTable(address);
    }

    /// <summary>
    /// 是否包含指定键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<bool> ContainsKey(TKey key)
    {
        return await TryFind(key, null);
    }

    async Task<bool> IDictionary.ContainsKey(object key)
    {
        return await ContainsKey((TKey)key);
    }

    /// <summary>
    /// 设置键值对
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Set(TKey key, long value)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await HashTable.Update(Database, await key.GetHashCode(Database),
            async objectAddress =>
        {
            bool isEquals = false;
            block.SetByRecordAddress(objectAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Read(Database, objectAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>();
                record.Read(buffer, 0);
                //Logger.Info($"Set Read ({objectAddress}) Key:{record.Key} Value:{record.Value}");
                isEquals = record.Key != null && await record.Key.Equals(Database, key);
            });
            return isEquals;
        },
            async () =>
        {
            var keyValueRecordAddress = await Database.AllocateRecord(KeyValueRecord<TKey>.GetFullName());
            block.SetByRecordAddress(keyValueRecordAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Write(Database, keyValueRecordAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>
                {
                    Key = key,
                    Value = value
                };
                //Logger.Info($"Set New ({keyValueRecordAddress}) Key:{key} Value:{value}");
                record.Write(buffer, 0);
                await Task.CompletedTask;
            });
            return keyValueRecordAddress;
        },
            async oldValue =>
        {
            block.SetByRecordAddress(oldValue, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Write(Database, oldValue, async buffer =>
            {
                var record = new KeyValueRecord<TKey>
                {
                    Key = key,
                    Value = value
                };
                //Logger.Info($"Set Update ({oldValue}) Key:{key} Value:{value}");
                record.Write(buffer, 0);
                await Task.CompletedTask;
            });
            return oldValue;
        });
        Database.Cache.StatisticalBlockPool.Enqueue(block);
    }

    async Task IDictionary.Set(object key, long value)
    {
        await Set((TKey)key, value);
    }

    public async Task<bool> TryFind(TKey key, Action<long>? onGet)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        var result = await HashTable.Get(Database, await key.GetHashCode(Database), async objectAddress =>
        {
            bool isEquals = false;
            block.SetByRecordAddress(objectAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Read(Database, objectAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>();
                record.Read(buffer, 0);
                //Logger.Info($"Get ({objectAddress}) Key:{record.Key} Value:{record.Value}");
                isEquals = record.Key != null && await record.Key.Equals(Database, key);
                if (isEquals)
                {
                    onGet?.Invoke(record.Value);
                }
            });
            return isEquals;
        });
        Database.Cache.StatisticalBlockPool.Enqueue(block);
        return result.Success;
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="outValue"></param>
    /// <returns></returns>
    public async Task<bool> TryGet(TKey key, Ref<long> outValue)
    {
        return await TryFind(key, value => outValue.Value = value);
    }

    async Task<bool> IDictionary.TryGet(object key, Ref<long> outValue)
    {
        return await TryGet((TKey)key, outValue);
    }

    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="outValue"></param>
    /// <param name="onAdd"></param>
    /// <returns></returns>
    public async Task TryGetOrAdd(TKey key, Ref<long> outValue, Func<TKey, Task<long>> onAdd)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await HashTable.Update(Database, await key.GetHashCode(Database),
            async objectAddress =>
        {
            bool isEquals = false;
            block.SetByRecordAddress(objectAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Read(Database, objectAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>();
                record.Read(buffer, 0);
                //Logger.Info($"Add Read({objectAddress}) Key:{record.Key} Value:{record.Value}");
                isEquals = record.Key != null && await record.Key.Equals(Database, key);
            });
            return isEquals;
        },
            async () =>
        {
            var keyValueRecordAddress = await Database.AllocateRecord(KeyValueRecord<TKey>.GetFullName());
            block.SetByRecordAddress(keyValueRecordAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Write(Database, keyValueRecordAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>
                {
                    Key = key,
                    Value = await onAdd(key)
                };
                outValue.Value = record.Value;
                //Logger.Info($"Add({keyValueRecordAddress}) Key:{key} Value:{record.Value}");
                record.Write(buffer, 0);
            });
            return keyValueRecordAddress;
        },
            async oldValue =>
        {
            block.SetByRecordAddress(oldValue, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Read(Database, oldValue, buffer =>
            {
                var record = new KeyValueRecord<TKey>();
                record.Read(buffer, 0);
                //Logger.Info($"Add Update({oldValue}) Key:{record.Key} Value:{record.Value}");
                outValue.Value = record.Value;
            });
            return oldValue;
        });
        Database.Cache.StatisticalBlockPool.Enqueue(block);
    }

    /// <summary>
    /// 移除指定键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveKey(TKey key)
    {
        var block = await Database.Cache.StatisticalBlockPool.Dequeue();
        await HashTable.Remove(Database, await key.GetHashCode(Database), async objectAddress =>
        {
            bool isEquals = false;
            block.SetByRecordAddress(objectAddress, KeyValueRecord<TKey>.GetSize());
            await block.RecordVisitor.Read(Database, objectAddress, async buffer =>
            {
                var record = new KeyValueRecord<TKey>();
                record.Read(buffer, 0);
                //Logger.Info($"Remove Read ({objectAddress}) Key:{record.Key} Value:{record.Value}");
                isEquals = record.Key != null && await record.Key.Equals(Database, key);
            });
            return isEquals;
        });
        Database.Cache.StatisticalBlockPool.Enqueue(block);
    }

    /// <summary>
    /// 移除指定键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    async Task IDictionary.RemoveKey(object key)
    {
        await RemoveKey((TKey)key);
    }
}
