using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;
public partial struct Json
{
    #region Array
    #region Getter
    /// <summary>
    /// Get the value by index, if not exist, throw exception
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public readonly Json this[int index]
    {
        get
        {
            return Get(index, Undefined);
        }
        set
        {
            Set(index, value);
        }
    }

    /// <summary>
    /// Get the value by index, if not exist, return default value
    /// </summary>
    /// <param name="index"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public readonly Json Get(int index, Json defaultValue)
    {
        if (IsArray)
        {
            var array = AsArray;
            if (index < 0 || index >= array.Count)
            {
                return defaultValue;
            }
            return new(array[index]);
        }
        else if (IsString)
        {
            var stringValue = AsString;
            if (index < 0 || index >= stringValue.Length)
            {
                return defaultValue;
            }
            return new(stringValue[index]);
        }
        else if (IsObject)
        {
            var key = index.ToString();
            return Get(key, defaultValue);
        }
        else
        {
            throw new Exception($"Unsupported type:{GetValueKind()}");
        }
    }

    /// <summary>
    /// Get the value by index, if not exist, throw exception
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public readonly Json Get(int index)
    {
        if (IsArray)
        {
            var array = AsArray;
            if (index < 0 || index >= array.Count)
            {
                throw new Exception($"Index out of range: {index}, {ToString()}");
            }
            return new(array[index]);
        }
        else if (IsString)
        {
            var stringValue = AsString;
            if (index < 0 || index >= stringValue.Length)
            {
                throw new Exception($"Index out of range: {index}, {ToString()}");
            }
            return new(stringValue[index]);
        }
        else
        {
            throw new Exception($"Unsupported type:{GetValueKind()}");
        }
    }
    /// <summary>
    /// Get the value by index, if not exist, create it
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public readonly Json GetOrCreateObject(int index)
    {
        AssertArray();
        var array = AsArray;
        if (index < 0 || index >= array.Count)
        {
            var result = new Dictionary<string, object?>();
            array.Add(result);
            return new(result);
        }
        return new(array[index]);
    }

    /// <summary>
    /// Get the value by index, if not exist, create it
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public readonly Json GetOrCreateArray(int index)
    {
        AssertArray();
        var array = AsArray;
        if (index < 0 || index >= array.Count)
        {
            var result = new List<object?>();
            array.Add(result);
            return new(result);
        }
        return new(array[index]);
    }
    #endregion

    #region Setter
    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, Json value)
    {
        AssertArray(self => self[index] = value.Node);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, JsonNode value)
    {
        AssertArray(self => self[index] = value);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, string value)
    {
        AssertArray(self => self[index] = value);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, int value)
    {
        AssertArray(self => self[index] = value);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, long value)
    {
        AssertArray(self => self[index] = value);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, double value)
    {
        AssertArray(self => self[index] = value);
    }

    /// <summary>
    /// Set value by index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Set(int index, bool value)
    {
        AssertArray(self => self[index] = value);
    }
    #endregion

    #region Add

    /// <summary>
    /// Add a object
    /// </summary>
    /// <returns></returns>
    public Json AddObject()
    {
        Json result = Null;
        AssertArray(self =>
        {
            var obj = NewObject();
            self.Add(obj.Node);
            result = obj;
        });
        return result;
    }

    /// <summary>
    /// Add a array
    /// </summary>
    /// <returns></returns>
    public Json AddArray()
    {
        Json result = Null;
        AssertArray(self =>
        {
            var array = NewArray();
            self.Add(array.Node);
            result = array;
        });
        return result;
    }

    /// <summary>
    /// 添加一个字节
    /// </summary>
    /// <param name="item"></param>
    public void Add(byte item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个int32
    /// </summary>
    /// <param name="item"></param>
    public void Add(int item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个float
    /// </summary>
    /// <param name="item"></param>
    public void Add(float item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个double
    /// </summary>
    /// <param name="item"></param>
    public void Add(double item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个string
    /// </summary>
    /// <param name="item"></param>
    public void Add(string item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个bool
    /// </summary>
    /// <param name="item"></param>
    public void Add(bool item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// 添加一个Json
    /// </summary>
    /// <param name="item"></param>
    public void Add(Json item)
    {
        AssertArray(self => self.Add(item.Node));
    }

    /// <summary>
    /// 添加一个long
    /// </summary>
    /// <param name="item"></param>
    public void Add(long item)
    {
        AssertArray(self => self.Add(item));
    }

    /// <summary>
    /// Add range from array
    /// </summary>
    /// <param name="array"></param>
    public void AddRange(Json array)
    {
        AssertArray(self =>
        {
            foreach (var item in array)
            {
                self.Add(item.Clone().Node);
            }
        });
    }

    /// <summary>
    /// 添加一个object
    /// </summary>
    /// <param name="item"></param>
    public void Add(object? item)
    {
        AssertArray(self => self.Add(item));
    }
    #endregion

    #region Insert
    /// <summary>
    /// 插入一个元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public readonly void Insert(int index, Json value)
    {
        AssertArray(self => self.Insert(index, value.Node));
    }
    #endregion

    /// <summary>
    /// Check array is contains value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(Json value)
    {
        if (IsArray)
        {
            return AsArray.Contains(value.Node);
        }
        else if (IsObject)
        {
            foreach (var item in AsObject)
            {
                if (new Json(item.Value) == value)
                {
                    return true;
                }
            }
        }
        else if (IsString)
        {
            return AsString.Contains(value.AsString);
        }
        return false;
    }

    /// <summary>
    /// Get the index of value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int IndexOf(Json value)
    {
        if (IsArray)
        {
            int index = 0;
            foreach (var item in AsArray)
            {
                if (new Json(item) == value)
                {
                    return index;
                }
                index++;
            }
        }
        else if (IsObject)
        {
            int index = 0;
            foreach (var item in AsObject)
            {
                if (new Json(item.Value) == value)
                {
                    return index;
                }
                index++;
            }
        }
        return -1;
    }

    /// <summary>
    /// Get the index of value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public int IndexOf(Json value, int startIndex)
    {
        if (IsArray)
        {
            int index = 0;
            foreach (var item in AsArray.Skip(startIndex))
            {
                if (new Json(item) == value)
                {
                    return index;
                }
                index++;
            }
        }
        else if (IsObject)
        {
            int index = 0;
            foreach (var item in AsObject.Skip(startIndex))
            {
                if (new Json(item.Value) == value)
                {
                    return index;
                }
                index++;
            }
        }
        return -1;
    }

    /// <summary>
    /// Get the last index of value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int LastIndexOf(Json value)
    {
        if (IsArray)
        {
            int index = AsArray.Count - 1;
            for (int i = index; i >= 0; i--)
            {
                if (new Json(AsArray[i]) == value)
                {
                    return i;
                }
            }
        }
        else if (IsObject)
        {
            int index = AsObject.Count - 1;
            for (int i = index; i >= 0; i--)
            {
                if (new Json(AsObject.ElementAt(i).Value) == value)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// Last index of value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public int LastIndexOf(Json value, int startIndex)
    {
        if (IsArray)
        {
            int index = AsArray.Count - 1;
            for (int i = index; i >= startIndex; i--)
            {
                if (new Json(AsArray[i]) == value)
                {
                    return i;
                }
            }
        }
        else if (IsObject)
        {
            int index = AsObject.Count - 1;
            for (int i = index; i >= startIndex; i--)
            {
                if (new Json(AsObject.ElementAt(i).Value) == value)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    /// <summary>
    /// 比较
    /// </summary>
    /// <param name="onSort"></param>
    public void Sort(Func<Json, Json, int> onSort) => AssertArray(self => self.Sort(onSort));

    /// <summary>
    /// 粘接
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="items"></param>
    public Json Splice(int start, int count, params Json[] items)
    {
        List<object?>? result = null;
        AssertArray(self =>
        {
            result = self.Splice(start, count, items);
        });
        return new(result);
    }

    /// <summary>
    /// 切片
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public Json Slice(int start, int end)
    {
        if (IsArray)
        {
            List<object?>? result = null;
            var self = AsArray;
            if (end < 0)
            {
                end = self.Count + end;
            }
            if (start < 0)
            {
                start = self.Count + start;
            }
            result = self.Slice(start, end);
            return new(result);
        }
        else if (IsString)
        {
            var self = AsString;
            if (end < 0)
            {
                end = self.Length + end;
            }
            if (start < 0)
            {
                start = self.Length + start;
            }
            return new(self[start..end]);
        }
        else
        {
            throw new Exception($"Unsupported type:{GetValueKind()}");
        }
    }

    /// <summary>
    /// 反序
    /// </summary>
    /// <returns></returns>
    public Json Reverse()
    {
        List<object?>? result = null;
        AssertArray(self =>
        {
            result = self.Reverse();
        });
        return new(result);
    }

    /// <summary>
    /// 拼接数组
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public Json Concat(params Json[] items)
    {
        List<object?>? result = null;
        AssertArray(self =>
        {
            result = self.Concat(items);
        });
        return new(result);
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void RemoveAt(int index)
    {
        AssertArray(self =>
        {
            if (index < 0 || index >= self.Count)
            {
                throw new IndexOutOfRangeException();
            }
            self.RemoveAt(index);
        });
    }

    #endregion
}
