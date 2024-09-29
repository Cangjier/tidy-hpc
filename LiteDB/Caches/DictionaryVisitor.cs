using System.Collections.Concurrent;
using TidyHPC.Common;
using TidyHPC.LiteDB.BasicValues;
using TidyHPC.LiteDB.DBTypes;
using TidyHPC.LiteDB.Dictionarys;
using TidyHPC.LiteDB.Metas;
using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Caches;

/// <summary>
/// 缓存字典访问器
/// </summary>
internal class DictionaryVisitor
{
    public DictionaryVisitor(Cache cache)
    {
        Cache = cache;
        Field[] fields =
        [
            new Field(){Type = FieldType.Char,ArrayLength = 8},
            new Field(){Type = FieldType.Char,ArrayLength = 16},
            new Field(){Type = FieldType.Char,ArrayLength = 32},
            new Field(){Type = FieldType.Char,ArrayLength = 64},
            new Field(){Type = FieldType.Char,ArrayLength = 128},
            new Field(){Type = FieldType.Char,ArrayLength = 256},
            new Field(){Type = FieldType.ReferneceString},
            new Field(){Type = FieldType.Guid},
            new Field(){Type = FieldType.DateTime},
            new Field(){Type = FieldType.MD5},
            new Field(){Type = FieldType.Int32},
            new Field(){Type = FieldType.Int64},
            new Field(){Type = FieldType.Float},
            new Field(){Type = FieldType.Double}
        ];
        foreach (var field in fields)
        {
            DictionaryDBType[GetIndex(field)] = NewWaitQueue(field);
            DictionaryArrayDBType[GetIndex(field)] = NewArrayWaitQueue(field);
            DictionaryHashSetDBType[GetIndex(field)] = NewHashSetWaitQueue(field);
            DictionarySmallHashSetDBType[GetIndex(field)] = NewSmallHashSetWaitQueue(field);
        }
    }

    public Cache Cache { get; }

    public Database Database => Cache.Database;

    internal ConcurrentDictionary<int, IWaitQueue> DictionaryDBType { get; } = new();

    internal ConcurrentDictionary<int, IWaitQueue> DictionaryArrayDBType { get; } = new();

    internal ConcurrentDictionary<int, IWaitQueue> DictionaryHashSetDBType { get; } = new();

    internal ConcurrentDictionary<int, IWaitQueue> DictionarySmallHashSetDBType { get; } = new();

    internal static Field MasterField = new Field()
    {
        Type = FieldType.Guid,
        ArrayLength = 1,
        MapType = FieldMapType.Master
    };

    internal IWaitQueue NewWaitQueue(Field field)
    {
        if (field.Type == FieldType.Char)
        {
            if (field.ArrayLength == 8) return new WaitQueue<Dictionary<DBString<Interger_8>>>().Initialize(64, () => new Dictionary<DBString<Interger_8>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 16) return new WaitQueue<Dictionary<DBString<Interger_16>>>().Initialize(64, () => new Dictionary<DBString<Interger_16>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 32) return new WaitQueue<Dictionary<DBString<Interger_32>>>().Initialize(64, () => new Dictionary<DBString<Interger_32>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 64) return new WaitQueue<Dictionary<DBString<Interger_64>>>().Initialize(64, () => new Dictionary<DBString<Interger_64>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 128) return new WaitQueue<Dictionary<DBString<Interger_128>>>().Initialize(64, () => new Dictionary<DBString<Interger_128>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 256) return new WaitQueue<Dictionary<DBString<Interger_256>>>().Initialize(64, () => new Dictionary<DBString<Interger_256>>()
            {
                Database = Database
            });
            throw new Exception("Unsupported type");
        }
        if (field.Type == FieldType.ReferneceString) return new WaitQueue<Dictionary<DBReferenceString>>().Initialize(64, () => new Dictionary<DBReferenceString>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Guid) return new WaitQueue<Dictionary<DBGuid>>().Initialize(64, () => new Dictionary<DBGuid>()
        {
            Database = Database
        });
        if (field.Type == FieldType.DateTime) return new WaitQueue<Dictionary<DBDateTime>>().Initialize(64, () => new Dictionary<DBDateTime>()
        {
            Database = Database
        });
        if (field.Type == FieldType.MD5) return new WaitQueue<Dictionary<DBMD5>>().Initialize(64, () => new Dictionary<DBMD5>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Int32) return new WaitQueue<DictionaryArray<DBInt32>>().Initialize(64, () => new DictionaryArray<DBInt32>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Int64) return new WaitQueue<DictionaryArray<DBInt64>>().Initialize(64, () => new DictionaryArray<DBInt64>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Float) return new WaitQueue<DictionaryArray<DBFloat32>>().Initialize(64, () => new DictionaryArray<DBFloat32>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Double) return new WaitQueue<DictionaryArray<DBFloat64>>().Initialize(64, () => new DictionaryArray<DBFloat64>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Boolean) return new WaitQueue<DictionaryArray<DBBoolean>>().Initialize(64, () => new DictionaryArray<DBBoolean>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Byte) return new WaitQueue<DictionaryArray<DBByte>>().Initialize(64, () => new DictionaryArray<DBByte>()
        {
            Database = Database
        });
        throw new Exception("Unsupported type");
    }

    internal IWaitQueue NewArrayWaitQueue(Field field)
    {
        if(field.Type == FieldType.Char)
        {
            if (field.ArrayLength == 8) return new WaitQueue<DictionaryArray<DBString<Interger_8>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_8>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 16) return new WaitQueue<DictionaryArray<DBString<Interger_16>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_16>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 32) return new WaitQueue<DictionaryArray<DBString<Interger_32>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_32>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 64) return new WaitQueue<DictionaryArray<DBString<Interger_64>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_64>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 128) return new WaitQueue<DictionaryArray<DBString<Interger_128>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_128>>()
            {
                Database = Database
            });
            if (field.ArrayLength == 256) return new WaitQueue<DictionaryArray<DBString<Interger_256>>>().Initialize(64, () => new DictionaryArray<DBString<Interger_256>>()
            {
                Database = Database
            });
            throw new Exception("Unsupported type");
        }
        if(field.Type == FieldType.ReferneceString) return new WaitQueue<DictionaryArray<DBReferenceString>>().Initialize(64, () => new DictionaryArray<DBReferenceString>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Guid) return new WaitQueue<DictionaryArray<DBGuid>>().Initialize(64, () => new DictionaryArray<DBGuid>()
        {
            Database = Database
        });
        if(field.Type == FieldType.DateTime) return new WaitQueue<DictionaryArray<DBDateTime>>().Initialize(64, () => new DictionaryArray<DBDateTime>()
        {
            Database = Database
        });
        if(field.Type == FieldType.MD5) return new WaitQueue<DictionaryArray<DBMD5>>().Initialize(64, () => new DictionaryArray<DBMD5>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int32) return new WaitQueue<DictionaryArray<DBInt32>>().Initialize(64, () => new DictionaryArray<DBInt32>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int64) return new WaitQueue<DictionaryArray<DBInt64>>().Initialize(64, () => new DictionaryArray<DBInt64>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Float) return new WaitQueue<DictionaryArray<DBFloat32>>().Initialize(64, () => new DictionaryArray<DBFloat32>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Double) return new WaitQueue<DictionaryArray<DBFloat64>>().Initialize(64, () => new DictionaryArray<DBFloat64>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Boolean) return new WaitQueue<DictionaryArray<DBBoolean>>().Initialize(64, () => new DictionaryArray<DBBoolean>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Byte) return new WaitQueue<DictionaryArray<DBByte>>().Initialize(64, () => new DictionaryArray<DBByte>()
        {
            Database = Database
        });
        throw new Exception("Unsupported type");
    }

    internal IWaitQueue NewHashSetWaitQueue(Field field)
    {
        if(field.Type == FieldType.Char)
        {
            if (field.ArrayLength == 8) return new WaitQueue<DictionaryHashSet<DBString<Interger_8>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_8>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 16) return new WaitQueue<DictionaryHashSet<DBString<Interger_16>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_16>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 32) return new WaitQueue<DictionaryHashSet<DBString<Interger_32>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_32>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 64) return new WaitQueue<DictionaryHashSet<DBString<Interger_64>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_64>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 128) return new WaitQueue<DictionaryHashSet<DBString<Interger_128>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_128>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 256) return new WaitQueue<DictionaryHashSet<DBString<Interger_256>, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBString<Interger_256>, Int64Value>()
            {
                Database = Database
            });
            throw new Exception("Unsupported type");
        }
        if(field.Type == FieldType.ReferneceString) return new WaitQueue<DictionaryHashSet<DBReferenceString, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBReferenceString, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Guid) return new WaitQueue<DictionaryHashSet<DBGuid, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBGuid, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.DateTime) return new WaitQueue<DictionaryHashSet<DBDateTime, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBDateTime, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.MD5) return new WaitQueue<DictionaryHashSet<DBMD5, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBMD5, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int32) return new WaitQueue<DictionaryHashSet<DBInt32, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBInt32, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int64) return new WaitQueue<DictionaryHashSet<DBInt64, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBInt64, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Float) return new WaitQueue<DictionaryHashSet<DBFloat32, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBFloat32, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Double) return new WaitQueue<DictionaryHashSet<DBFloat64, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBFloat64, Int64Value>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Boolean) return new WaitQueue<DictionaryHashSet<DBBoolean, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBBoolean, Int64Value>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Byte) return new WaitQueue<DictionaryHashSet<DBByte, Int64Value>>().Initialize(64, () => new DictionaryHashSet<DBByte, Int64Value>()
        {
            Database = Database
        });
        throw new Exception("Unsupported type");
    }

    internal IWaitQueue NewSmallHashSetWaitQueue(Field field)
    {
        if(field.Type == FieldType.Char)
        {
            if (field.ArrayLength == 8) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_8>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_8>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 16) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_16>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_16>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 32) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_32>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_32>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 64) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_64>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_64>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 128) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_128>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_128>, Int64Value>()
            {
                Database = Database
            });
            if (field.ArrayLength == 256) return new WaitQueue<DictionarySmallHashSet<DBString<Interger_256>, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBString<Interger_256>, Int64Value>()
            {
                Database = Database
            });
            throw new Exception("Unsupported type");
        }
        if(field.Type == FieldType.ReferneceString) return new WaitQueue<DictionarySmallHashSet<DBReferenceString, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBReferenceString, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Guid) return new WaitQueue<DictionarySmallHashSet<DBGuid, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBGuid, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.DateTime) return new WaitQueue<DictionarySmallHashSet<DBDateTime, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBDateTime, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.MD5) return new WaitQueue<DictionarySmallHashSet<DBMD5, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBMD5, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int32) return new WaitQueue<DictionarySmallHashSet<DBInt32, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBInt32, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Int64) return new WaitQueue<DictionarySmallHashSet<DBInt64, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBInt64, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Float) return new WaitQueue<DictionarySmallHashSet<DBFloat32, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBFloat32, Int64Value>()
        {
            Database = Database
        });
        if(field.Type == FieldType.Double) return new WaitQueue<DictionarySmallHashSet<DBFloat64, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBFloat64, Int64Value>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Boolean) return new WaitQueue<DictionarySmallHashSet<DBBoolean, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBBoolean, Int64Value>()
        {
            Database = Database
        });
        if (field.Type == FieldType.Byte) return new WaitQueue<DictionarySmallHashSet<DBByte, Int64Value>>().Initialize(64, () => new DictionarySmallHashSet<DBByte, Int64Value>()
        {
            Database = Database
        });
        throw new Exception("Unsupported type");
    }

    internal int GetIndex(Field field)
    {
        if (field.Type == FieldType.Char) return field.ArrayLength;
        if (field.Type == FieldType.ReferneceString) return -1;
        if (field.Type == FieldType.Guid) return -2;
        if (field.Type == FieldType.DateTime) return -3;
        if (field.Type == FieldType.MD5) return -4;
        if (field.Type == FieldType.Int32) return -5;
        if (field.Type == FieldType.Int64) return -6;
        if (field.Type == FieldType.Float) return -7;
        if (field.Type == FieldType.Double) return -8;
        if (field.Type == FieldType.Boolean) return -9;
        if (field.Type == FieldType.Byte) return -10;
        throw new Exception("Unsupported type");
    }

    internal async Task<IDictionary> Dequeue(Field field)
    {
        return (IDictionary)await DictionaryDBType[GetIndex(field)].Dequeue();
    }

    internal async Task<IDictionaryArray> DequeueArray(Field field)
    {
        return (IDictionaryArray)await DictionaryArrayDBType[GetIndex(field)].Dequeue();
    }

    internal async Task<IDictionaryHashSet<Int64Value>> DequeueHashSet(Field field)
    {
        return (IDictionaryHashSet<Int64Value>)await DictionaryHashSetDBType[GetIndex(field)].Dequeue();
    }

    internal async Task<IDictionaryHashSet<Int64Value>> DequeueSmallHashSet(Field field)
    {
        return (IDictionaryHashSet<Int64Value>)await DictionarySmallHashSetDBType[GetIndex(field)].Dequeue();
    }

    internal void Enqueue(Field field, IDictionary dictionary)
    {
        DictionaryDBType[GetIndex(field)].Enqueue(dictionary);
    }

    internal void EnqueueArray(Field field, IDictionaryArray dictionary)
    {
        DictionaryArrayDBType[GetIndex(field)].Enqueue(dictionary);
    }

    internal void EnqueueHashSet(Field field, IDictionaryHashSet<Int64Value> dictionary)
    {
        DictionaryHashSetDBType[GetIndex(field)].Enqueue(dictionary);
    }

    internal void EnqueueSmallHashSet(Field field, IDictionaryHashSet<Int64Value> dictionary)
    {
        DictionarySmallHashSetDBType[GetIndex(field)].Enqueue(dictionary);
    }

    internal DBType NewDBType(Field field,object value)
    {
        if (field.Type == FieldType.Char)
        {
            if(field.ArrayLength == 8) return new DBString<Interger_8>((string)value);
            if(field.ArrayLength == 16) return new DBString<Interger_16>((string)value);
            if(field.ArrayLength == 32) return new DBString<Interger_32>((string)value);
            if(field.ArrayLength == 64) return new DBString<Interger_64>((string)value);
            if(field.ArrayLength == 128) return new DBString<Interger_128>((string)value);
            if(field.ArrayLength == 256) return new DBString<Interger_256>((string)value);
            throw new Exception("Unsupported type");
        }
        else if(field.Type == FieldType.ReferneceString) return new DBReferenceString((long)value);
        else if(field.Type == FieldType.Guid) return new DBGuid((Guid)value);
        else if(field.Type == FieldType.DateTime) return new DBDateTime((long)value);
        else if(field.Type == FieldType.MD5) return new DBMD5((byte[])value);
        else if(field.Type == FieldType.Int32) return new DBInt32((int)value);
        else if(field.Type == FieldType.Int64) return new DBInt64((long)value);
        else if(field.Type == FieldType.Float) return new DBFloat32((float)value);
        else if(field.Type == FieldType.Double) return new DBFloat64((double)value);
        else if (field.Type == FieldType.Boolean) return new DBBoolean((bool)value);
        else if (field.Type == FieldType.Byte) return new DBByte((byte)value);
        throw new Exception("Unsupported type");
    }

    internal async Task<long> Get(Field field,long mappingHashTableAddress,object key)
    {
        var dictionary = await Dequeue(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            Ref<long> recordAddress = new(0);
            if (await dictionary.TryGet(NewDBType(field,key), recordAddress))
            {
                return recordAddress.Value;
            }
            else
            {
                throw new Exception("Index not found");
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(field, dictionary);
        }
    }

    internal async Task<long> GetGuid(long mappingHashTableAddress, object key)
        => await Get(MasterField, mappingHashTableAddress, key);

    internal async Task<bool> ContainsKey(Field field, long mappingHashTableAddress, object key)
    {
        var dictionary = await Dequeue(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            Ref<long> recordAddress = new(0);
            if (await dictionary.TryGet(NewDBType(field,key), recordAddress))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            Enqueue(field, dictionary);
        }
    }

    internal async Task<bool> ContainsGuid(long mappingHashTableAddress, object key)
        => await ContainsKey(MasterField, mappingHashTableAddress, key);

    internal async Task Set(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await Dequeue(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Set(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(field, dictionary);
        }
    }

    internal async Task RemoveKey(Field field,long mappingHashTableAddress,object key)
    {
        var dictionary = await Dequeue(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.RemoveKey(NewDBType(field,key));
        }
        catch
        {
            throw;
        }
        finally
        {
            Enqueue(field, dictionary);
        }
    }

    internal async Task AddToArray(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueArray(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Add(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueArray(field, dictionary);
        }
    }

    internal async Task<bool> ContainsInArray(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueArray(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            return await dictionary.Contains(NewDBType(field, key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueArray(field, dictionary);
        }
    }

    internal async Task RemoveFromArray(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueArray(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Remove(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueArray(field, dictionary);
        }
    }

    internal async Task GetArray(Field field,long mappingHashTableAddress,object key,Action<long> onValue)
    {
        var dictionary = await DequeueArray(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Get(NewDBType(field, key), onValue);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueArray(field, dictionary);
        }
    }
    
    internal async Task AddToHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Add(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueHashSet(field, dictionary);
        }
    }

    internal async Task<bool> ContainsInHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            return await dictionary.Contains(NewDBType(field, key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueHashSet(field, dictionary);
        }
    }

    internal async Task RemoveFromHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Remove(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueHashSet(field, dictionary);
        }
    }

    internal async Task GetHashSet(Field field,long mappingHashTableAddress,object key,Action<long> onValue)
    {
        var dictionary = await DequeueHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Get(NewDBType(field, key), value => onValue(value));
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueHashSet(field, dictionary);
        }
    }

    internal async Task AddToSmallHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueSmallHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Add(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueSmallHashSet(field, dictionary);
        }
    }

    internal async Task<bool> ContainsInSmallHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueSmallHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            return await dictionary.Contains(NewDBType(field, key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueSmallHashSet(field, dictionary);
        }
    }

    internal async Task RemoveFromSmallHashSet(Field field,long mappingHashTableAddress,object key,long value)
    {
        var dictionary = await DequeueSmallHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Remove(NewDBType(field,key), value);
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueSmallHashSet(field, dictionary);
        }
    }

    internal async Task GetSmallHashSet(Field field,long mappingHashTableAddress,object key,Action<long> onValue)
    {
        var dictionary = await DequeueSmallHashSet(field);
        try
        {
            dictionary.SetHashTable(mappingHashTableAddress);
            await dictionary.Get(NewDBType(field, key), value => onValue(value));
        }
        catch
        {
            throw;
        }
        finally
        {
            EnqueueSmallHashSet(field, dictionary);
        }
    }
}
