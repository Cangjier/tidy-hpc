using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;
public partial struct Json
{
    #region Object

    /// <summary>
    /// Remove key
    /// </summary>
    /// <param name="key"></param>
    public void RemoveKey(string key)
    {
        AssertObject(self => self.Remove(key));
    }

    /// <summary>
    /// 键名集合
    /// </summary>
    public string[] Keys
    {
        get
        {
            AssertObject();
            return AsObject.Keys;
        }
    }

    /// <summary>
    /// 值集合
    /// </summary>
    public object?[] Values
    {
        get
        {
            AssertObject();
            return AsObject.Values;
        }
    }

    #region Setter
    /// <summary>
    /// Set string value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, string? value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set byte value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, byte value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set int value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, int value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set char value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, char value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set long value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, long value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set float value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Json Set(string key, float value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set double value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, double value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set bool value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, bool value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set Guid value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, Guid value)
    {
        AssertObject(self => self[key] = value.ToString());
        return this;
    }

    /// <summary>
    /// Set DateTime value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, DateTime value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set TimeSpan value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, TimeSpan value)
    {
        AssertObject(self => self[key] = value.ToString());
        return this;
    }

    /// <summary>
    /// Set Json value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, Json value)
    {
        AssertObject(self => self[key] = value.Node);
        return this;
    }

    /// <summary>
    /// Set JsonNode value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, JsonNode value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }

    /// <summary>
    /// Set value, return self
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public Json Set(string key, object? value)
    {
        AssertObject(self => self[key] = value);
        return this;
    }
    #endregion

    #region Senior Setter

    /// <summary>
    /// Set value by path, if not exist, create it
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    /// <exception cref="Exception"></exception>
    public void SetByPath(JsonIndex[] path, Json value)
    {
        var last = this;
        for (int i = 0; i < path.Length - 1; i++)
        {
            var pathIndex = path[i];
            var nextIndex = path[i + 1];
            if (pathIndex.Key is not null)
            {
                if (nextIndex.IsObject)
                {
                    last = last.GetOrCreateObject(pathIndex.Key);
                }
                else if (nextIndex.IsArray)
                {
                    last = last.GetOrCreateArray(pathIndex.Key);
                }
                else
                {
                    throw new Exception("Invalid JsonIndex");
                }
            }
            else if (pathIndex.Index is not null)
            {
                if (nextIndex.IsObject)
                {
                    last = last.GetOrCreateObject(pathIndex.Index.Value);
                }
                else if (nextIndex.IsArray)
                {
                    last = last.GetOrCreateArray(pathIndex.Index.Value);
                }
                else
                {
                    throw new Exception("Invalid JsonIndex");
                }
            }
            else
            {
                throw new Exception("Invalid JsonIndex");
            }
        }
        var lastIndex = path[^1];
        if (lastIndex.Key is not null)
        {
            last.Set(lastIndex.Key, value);
        }
        else if (lastIndex.Index is not null)
        {
            last.Set(lastIndex.Index.Value, value);
        }
        else
        {
            throw new Exception("Invalid JsonIndex");
        }
    }

    /// <summary>
    /// Set value by path, if not exist, create it
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    public void SetByPathString(IEnumerable<string> path, Json value) => SetByPath(path.Select(item => new JsonIndex(item)).ToArray(), value);

    /// <summary>
    /// Set value by path, if not exist, create it
    /// </summary>
    /// <param name="path"></param>
    /// <param name="value"></param>
    public void SetByPathInt32(IEnumerable<int> path, Json value) => SetByPath(path.Select(item => new JsonIndex(item)).ToArray(), value);
    #endregion

    #region Getter

    /// <summary>
    /// get the value by key, if not exist, throw exception
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Json this[string key]
    {
        get
        {
            return Get(key, Undefined);
        }
        set
        {
            Set(key, value);
        }
    }

    /// <summary>
    /// get the value by key, if not exist, throw exception
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Json this[Json index]
    {
        get
        {
            if (index.IsString) return Get(index.AsString, Undefined);
            else if (index.IsNumber) return Get(index.ToInt32, Undefined);
            else
            {
                return Undefined;
            }
        }
        set
        {
            if (index.IsString) Set(index.AsString, value);
            else if (index.IsNumber) Set(index.AsInt32, value);
            else
            {
                throw new Exception("Invalid index");
            }
        }
    }

    /// <summary>
    /// Get the value by key, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    public static Json op_GetMember(Json self, string key)
    {
        if (self.IsObject)
        {
            if (self.ContainsKey(key))
            {
                return self.Get(key);
            }
        }
        if (self.Node is null) return Undefined;
        var nodeType = self.Node.GetType();
        var fields = nodeType.GetFields();
        foreach (var field in fields)
        {
            if (field.Name == key)
            {
                return new(field.GetValue(self.Node));
            }
        }
        var properties = nodeType.GetProperties();
        foreach (var property in properties)
        {
            if (property.Name == key)
            {
                return new(property.GetValue(self.Node));
            }
        }
        return Undefined;
    }

    /// <summary>
    /// Set the value by key, if not exist, create it
    /// </summary>
    /// <param name="self"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Json op_SetMember(Json self, string key, Json value)
    {
        if (self.IsObject)
        {
            self.Set(key, value);
            return value;
        }
        if (self.Node is null) return value;
        var nodeType = self.Node.GetType();
        var fields = nodeType.GetFields();
        foreach (var field in fields)
        {
            if (field.Name == key)
            {
                if(field.FieldType == typeof(Json))
                {
                    field.SetValue(self.Node, value);
                }
                else
                {
                    field.SetValue(self.Node, value.Node);
                }
                return value;
            }
        }
        var properties = nodeType.GetProperties();
        foreach (var property in properties)
        {
            if (property.Name == key)
            {
                if (property.PropertyType == typeof(Json))
                {
                    property.SetValue(self.Node, value);
                }
                else
                {
                    property.SetValue(self.Node, value.Node);
                }
                return value;
            }
        }
        return value;
    }

    /// <summary>
    /// op_Invoke 
    /// </summary>
    public static Func<Json, string, Json[], Json>? _op_Invoke { get; set; } = null;

    /// <summary>
    /// invoke the value by key
    /// </summary>
    /// <param name="self"></param>
    /// <param name="objects"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Json op_Invoke(Json self, string name, params Json[] objects)
    {
        if (_op_Invoke is not null)
        {
            return _op_Invoke(self, name, objects);
        }
        return Undefined;
    }

    /// <summary>
    /// Invoke
    /// </summary>
    public static Func<Json, Json[], Json>? _Invoke { get; set; } = null;

    /// <summary>
    /// Invoke
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public Json Invoke(params Json[] objects)
    {
        if (_Invoke is not null)
        {
            return _Invoke(this, objects);
        }
        else return Undefined;
    }

    /// <summary>
    /// Get the value by key, if not exist, create it
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Json GetOrCreateObject(string key)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            return new(obj[key]);
        }
        var result = new Dictionary<string, object?>();
        obj[key] = result;
        return new(result);
    }

    /// <summary>
    /// Get the value by key, if not exist, create it
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Json GetOrCreateArray(string key)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            return new(obj[key]);
        }
        var result = new List<object?>();
        obj[key] = result;
        return new(result);
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Json Get(string key, Json defaultValue)
    {
        if (!IsObject) return defaultValue;
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            return new(obj[key]);
        }
        return defaultValue;
    }

    /// <summary>
    /// Get or create the value, if not exist, create it
    /// </summary>
    /// <param name="key"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    public Json GetOrCreate(string key, Func<Json> onDefaultValue)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            return new(obj[key]);
        }
        var result = onDefaultValue();
        obj[key] = result.Node;
        return result;
    }

    /// <summary>
    /// 获取或创建字符串值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    public string? GetOrCreateString(string key, Func<string?> onDefaultValue)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            var value = obj[key];
            if (new Json(value).IsString)
            {
                return new Json(value).AsString;
            }
            else
            {
                throw new Exception("Invalid value type");
            }
        }
        var result = onDefaultValue();
        obj[key] = result;
        return result;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Guid GetGuid(string key, Guid defaultValue)
    {
        AssertObject();
        var result = Get(key);
        if (result.IsString)
        {
            return Guid.Parse(result.AsString);
        }
        return defaultValue;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public DateTime GetDateTime(string key, DateTime defaultValue)
    {
        AssertObject();
        var result = Get(key);
        if (result.IsString)
        {
            return DateTime.Parse(result.AsString);
        }
        return defaultValue;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TimeSpan GetTimeSpan(string key, TimeSpan defaultValue)
    {
        AssertObject();
        var result = Get(key);
        if (result.IsString)
        {
            return TimeSpan.Parse(result.AsString);
        }
        return defaultValue;
    }

    /// <summary>
    /// Try get the value, if not exist, return false
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetString(string key, out string value)
    {
        var result = Get(key, Null);
        if (result.IsString)
        {
            value = result.AsString;
            return true;
        }
        value = string.Empty;
        return false;
    }

    /// <summary>
    /// Get the value, if not exist, throw exception
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Json Get(string key)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            return new(obj[key]);
        }
        else
        {
            throw new Exception($"Key not found: {key}, {ToString()}");
        }
    }

    /// <summary>
    /// Try get the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGet(string key, out Json value)
    {
        AssertObject();
        var obj = AsObject;
        if (obj.ContainsKey(key))
        {
            value = new(obj[key]);
            return true;
        }
        value = Null;
        return false;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string Read(string key, string defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsString;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string ReadWithKeys(string[] keys, string defaultValue)
    {
        if (!IsObject) return defaultValue;
        var obj = AsObject;
        foreach (var key in keys)
        {
            if (obj.ContainsKey(key))
            {
                return new Json(obj[key]).AsString;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public int ReadWithKeys(string[] keys, int defaultValue)
    {
        if (!IsObject) return defaultValue;
        var obj = AsObject;
        foreach (var key in keys)
        {
            if (obj.ContainsKey(key))
            {
                return new Json(obj[key]).AsInt32;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public byte Read(string key, byte defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsByte;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public int Read(string key, int defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.ToInt32;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public long Read(string key, long defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsInt64;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public float Read(string key, float defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsFloat;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public double Read(string key, double defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsDouble;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public bool Read(string key, bool defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : result.AsBoolean;
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Guid Read(string key, Guid defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : Guid.Parse(result.AsString);
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TimeSpan Read(string key, TimeSpan defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : TimeSpan.Parse(result.AsString);
    }

    /// <summary>
    /// Read the value, if not exist, return default value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public DateTime Read(string key, DateTime defaultValue)
    {
        var result = Get(key, Null);
        return result.IsNull ? defaultValue : DateTime.Parse(result.AsString);
    }

    /// <summary>
    /// Get the value, if not exist, throw exception
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Json GetByPath(IEnumerable<JsonIndex> path)
    {
        var last = this;
        foreach (var index in path)
        {
            if (index.Key is not null)
            {
                last = last.Get(index.Key);
            }
            else if (index.Index is not null)
            {
                last = last.Get(index.Index.Value);
            }
            else
            {
                throw new Exception("Invalid JsonIndex");
            }
        }
        return last;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="path"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Json GetByPath(IEnumerable<JsonIndex> path, Json defaultValue)
    {
        var last = this;
        foreach (var index in path)
        {
            if (index.Key is not null)
            {
                last = last.Get(index.Key);
            }
            else if (index.Index is not null)
            {
                last = last.Get(index.Index.Value);
            }
            else
            {
                return defaultValue;
            }
        }
        return last;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Json GetByPath(IEnumerable<JsonIndex> path, Func<Json> onDefaultValue)
    {
        var last = this;
        foreach (var index in path)
        {
            if (index.Key is not null)
            {
                last = last.Get(index.Key);
            }
            else if (index.Index is not null)
            {
                last = last.Get(index.Index.Value);
            }
            else
            {
                return onDefaultValue();
            }
        }
        return last;
    }

    /// <summary>
    /// Get the value, if not exist, return default value
    /// </summary>
    /// <param name="path"></param>
    /// <param name="defaultValue"></param>
    /// <param name="onException"></param>
    /// <returns></returns>
    public Json GetByPath(IEnumerable<JsonIndex> path, Json defaultValue, Action<Exception>? onException = null)
    {
        try
        {
            var last = this;
            foreach (var index in path)
            {
                if (index.Key is not null)
                {
                    last = last.Get(index.Key);
                }
                else if (index.Index is not null)
                {
                    last = last.Get(index.Index.Value);
                }
                else
                {
                    throw new Exception("Invalid JsonIndex");
                }
            }
            return last;
        }
        catch (Exception e)
        {
            onException?.Invoke(e);
            return defaultValue;
        }
    }

    #endregion

    /// <summary>
    /// Contains key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        if (IsObject)
        {
            return AsObject.ContainsKey(key);
        }
        return false;
    }
    #endregion
}
