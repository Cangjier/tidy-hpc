using System.Collections;

namespace TidyHPC.LiteJson;
public partial struct Json : IDictionary
{
    /// <inheritdoc/>
    public bool IsFixedSize => false;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    ICollection IDictionary.Keys => Keys;

    /// <inheritdoc/>
    ICollection IDictionary.Values => Values;

    /// <inheritdoc/>
    public bool IsSynchronized => false;

    /// <inheritdoc/>
    public object SyncRoot => throw new NotImplementedException();

    /// <inheritdoc/>
    public object? this[object key]
    {
        get
        {
            if (key is string keyString)
            {
                return this[new Json(keyString)];
            }
            else if(key is Json keyJson)
            {
                return this[keyJson];
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        set
        {
            if(key is string keyString)
            {
                this[keyString] = new Json(value);
            }
            else if(key is Json keyJson)
            {
                this[keyJson] = new Json(value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <inheritdoc/>
    public void Add(object key, object? value)
    {
        AssertObject();
        if(key is string keyString)
        {
            AsObject.Add(keyString, value);
        }
        else if(key is Json keyJson)
        {
            if(keyJson.IsString)
            {
                AsObject.Add(keyJson.AsString, value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc/>
    public bool Contains(object key)
    {
        return Contains(new Json(key));
    }

    /// <inheritdoc/>
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return new DictionaryEnumerator(this);
    }

    /// <inheritdoc/>
    public void Remove(object key)
    {
        if (key is string keyString)
        {
            AsObject.Remove(keyString);
        }
        else if (key is Json keyJson)
        {
            if (keyJson.IsString)
            {
                AsObject.Remove(keyJson.AsString);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc/>
    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }
}

internal class DictionaryEnumerator : IDictionaryEnumerator
{
    DictionaryEntry[] items;
    Int32 index = -1;
    //构造方法，用于获得SimpleDictionary类的所有元素
    public DictionaryEnumerator(ObjectWrapper sd)
    {
        items = sd.Select(item => new DictionaryEntry()
        {
            Key = item.Key,
            Value = item.Value
        }).ToArray();
    }
    public object Current
    {
        get { return items[index]; }
    }
    public DictionaryEntry Entry
    {
        get { return (DictionaryEntry)Current; }
    }
    public object Key { get { return items[index].Key; } }
    public object? Value { get { return items[index].Value; } }
    public bool MoveNext()
    {
        if (index < items.Length - 1) { index++; return true; }
        return false;
    }
    public void Reset()
    {
        index = -1;
    }
}