using TidyHPC.Common;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.Dictionarys;

internal interface IDictionaryHashSet<TValue>
    where TValue : struct, IValue<TValue>
{
    internal void SetHashTable(long address);

    Task Add(object key, TValue value);

    Task<bool> Contains(object key, TValue value);

    Task Remove(object key, TValue value);

    Task Get(object key, Action<TValue> onItem);
}

internal class DictionaryHashSet<TKey, TValue> : IDictionaryHashSet<TValue>
    where TKey : DBType, new()
    where TValue : struct, IValue<TValue>
{
    private Dictionary<TKey> Dictionary { get; } = new();

    public HashTable<Int64Value> HashTable
    {
        get => Dictionary.HashTable;
    }

    public Database Database
    {
        get => Dictionary.Database;
        set => Dictionary.Database = value;
    }

    public void SetHashTable(long address)
    {
        Dictionary.SetHashTable(address);
    }

    void IDictionaryHashSet<TValue>.SetHashTable(long address)
    {
        SetHashTable(address);
    }

    public async Task Add(TKey key, TValue value)
    {
        Ref<long> outValue = new(0);
        var valueHashTable = new HashTable<TValue>();
        if (await Dictionary.TryGet(key, outValue))
        {
            valueHashTable.SetAddress(outValue.Value);
        }
        else
        {
            outValue.Value = await Database.AllocateHashTable();
            await Dictionary.Set(key, outValue.Value);
            valueHashTable.SetAddress(outValue.Value);
        }
        await valueHashTable.TryAdd(Database, await value.GetHashCode(Database), async item =>
        {
            await Task.CompletedTask;
            return item == value;
        }, async () =>
        {
            await Task.CompletedTask;
            return value;
        });
    }

    async Task IDictionaryHashSet<TValue>.Add(object key, TValue value)
    {
        await Add((TKey)key, value);
    }

    public async Task Get(TKey key, Action<TValue> onItem)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            var hashSet = new HashTable<TValue>();
            hashSet.SetAddress(outValue.Value);
            await hashSet.GetValues(Database, item =>
            {
                onItem(item);
            });
        }
    }

    async Task IDictionaryHashSet<TValue>.Get(object key, Action<TValue> onItem)
    {
        await Get((TKey)key, onItem);
    }

    public async Task<bool> Contains(TKey key, TValue value)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            var hashSet = new HashTable<TValue>();
            hashSet.SetAddress(outValue.Value);
            return await hashSet.Contains(Database, await value.GetHashCode(Database), async item =>
            {
                await Task.CompletedTask;
                return item == value;
            });
        }
        return false;
    }

    async Task<bool> IDictionaryHashSet<TValue>.Contains(object key, TValue value)
    {
        return await Contains((TKey)key, value);
    }

    public async Task Remove(TKey key,TValue value)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            var hashSet = new HashTable<TValue>();
            hashSet.SetAddress(outValue.Value);
            await hashSet.Remove(Database, await value.GetHashCode(Database), async item =>
            {
                await Task.CompletedTask;
                return item == value;
            });
        }
    }

    async Task IDictionaryHashSet<TValue>.Remove(object key, TValue value)
    {
        await Remove((TKey)key, value);
    }
}
