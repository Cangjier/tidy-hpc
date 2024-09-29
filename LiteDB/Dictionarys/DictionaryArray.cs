using TidyHPC.Common;
using TidyHPC.LiteDB.Arrays;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.LiteDB.Hashes;

namespace TidyHPC.LiteDB.Dictionarys;

internal interface IDictionaryArray
{
    internal void SetHashTable(long address);

    Task Add(object key, long value);

    Task<bool> Contains(object key, long value);

    Task Remove(object key, long value);

    Task Get(object key, Action<long> onValue);
}


internal class DictionaryArray<TKey>:IDictionaryArray
    where TKey : DBType, new()
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

    void IDictionaryArray.SetHashTable(long address)
    {
        SetHashTable(address);
    }

    public async Task Get(TKey key, Action<long> onValue)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            using ArrayProcessor<Int64Value> arrayProcessor = new(Database, outValue.Value);
            await arrayProcessor.Get(value => onValue(value));
        }
    }

    async Task IDictionaryArray.Get(object key, Action<long> onValue)
    {
        await Get((TKey)key, onValue);
    }

    public async Task Add(TKey key, long value)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            using ArrayProcessor<Int64Value> arrayProcessor = new(Database, outValue.Value);
            await arrayProcessor.Add(value);
        }
        else
        {
            outValue.Value = await Database.AllocateRecord(ArrayRecord<Int64Value>.InterfaceName);
            using ArrayProcessor<Int64Value> arrayProcessor = new(Database, outValue.Value);
            await arrayProcessor.Add(value);
            await Dictionary.Set(key, outValue.Value);
        }
    }

    async Task IDictionaryArray.Add(object key, long value)
    {
        await Add((TKey)key, value);
    }

    public async Task<bool> Contains(TKey key, long value)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            using ArrayProcessor<Int64Value> arrayProcessor = new(Database, outValue.Value);
            return await arrayProcessor.Contains(value);
        }
        return false;
    }

    async Task<bool> IDictionaryArray.Contains(object key, long value)
    {
        return await Contains((TKey)key, value);
    }

    public async Task Remove(TKey key, long value)
    {
        Ref<long> outValue = new(0);
        if (await Dictionary.TryGet(key, outValue))
        {
            using ArrayProcessor<Int64Value> arrayProcessor = new(Database, outValue.Value);
            await arrayProcessor.Remove(value);
        }
    }

    async Task IDictionaryArray.Remove(object key, long value)
    {
        await Remove((TKey)key, value);
    }
}
