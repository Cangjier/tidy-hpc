using System.Collections;
using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;

/// <summary>
/// Array Wrapper
/// </summary>
/// <param name="target"></param>
public struct ArrayWrapper(object? target):IDisposable,IEnumerable<object?>
{
    /// <summary>
    /// The target
    /// </summary>
    public object? Target { get; } = target;

    /// <summary>
    /// Get the count of the array
    /// </summary>
    public int Count
    {
        get
        {
            if (Target is IList list) return list.Count;
            if (Target is Array array) return array.Length;
            if (Target is IDictionary dictionary) return dictionary.Count;
            if (Target is JsonArray jsonArray) return jsonArray.Count;
            return 0;
        }
    }

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object? this[int index]
    {
        get
        {
            if (Target is IList list) return list[index];
            if (Target is Array array) return array.GetValue(index);
            if (Target is JsonArray jsonArray) return jsonArray[index];
            return null;
        }
        set
        {
            if (value is Json json) value = json.Node;
            else if (value is ObjectWrapper objectWrapper) value = objectWrapper.Target;
            else if (value is ArrayWrapper arrayWrapper) value = arrayWrapper.Target;

            if (Target is IList list) list[index] = value;
            else if (Target is Array array) array.SetValue(value, index);
            else if (Target is JsonArray jsonArray)
            {
                var node = (JsonNode?)value;
                if (node?.Parent != null) node = node.DeepClone();
                jsonArray[index] = node;
            }
        }
    }

    /// <summary>
    /// 添加一个元素
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public ArrayWrapper Add(object? value)
    {
        if (value is Json json) value = json.Node;
        else if (value is ObjectWrapper objectWrapper) value = objectWrapper.Target;
        else if (value is ArrayWrapper arrayWrapper) value = arrayWrapper.Target;

        if (Target is IList list) list.Add(value);
        else if (Target is JsonArray jsonArray)
        {
            var node = (JsonNode?)value;
            if (node?.Parent != null) node = node.DeepClone();
            jsonArray.Add(node);
        }
        else throw new NotImplementedException();
        return this;
    }

    /// <summary>
    /// 插入一个元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Insert(int index,object? value)
    {
        if (value is Json json) value = json.Node;
        else if (value is ObjectWrapper objectWrapper) value = objectWrapper.Target;
        else if (value is ArrayWrapper arrayWrapper) value = arrayWrapper.Target;

        if (Target is IList list) list.Insert(index, value);
        else if (Target is JsonArray jsonArray)
        {
            var node = (JsonNode?)value;
            if (node?.Parent != null) node = node.DeepClone();
            jsonArray.Insert(index, node);
        }
        else throw new NotImplementedException();
    }

    /// <summary>
    /// 包含
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(object? value)
    {
        if (value is Json json) value = json.Node;
        else if (value is ObjectWrapper objectWrapper) value = objectWrapper.Target;
        else if (value is ArrayWrapper arrayWrapper) value = arrayWrapper.Target;

        if (Target is IList list)
        {
            foreach (var item in list)
            {
                if (JsonUtil.DeepEquals(item, value)) return true;
            }
            return false;
        }
        if (Target is Array array)
        {
            foreach (var item in array)
            {
                if (JsonUtil.DeepEquals(item, value)) return true;
            }
            return false;
        }
        if (Target is JsonArray jsonArray) return jsonArray.Contains(value);
        return false;
    }

    /// <summary>
    /// 批量添加元素
    /// </summary>
    /// <param name="items"></param>
    public ArrayWrapper AddRange(IEnumerable<object?> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
        return this;
    }

    /// <summary>
    /// 清空
    /// </summary>
    public ArrayWrapper Clear()
    {
        if (Target is IList list) list.Clear();
        else if (Target is Array array) Array.Clear(array, 0, array.Length);
        else if (Target is IDictionary dictionary) dictionary.Clear();
        else if (Target is JsonArray jsonArray) jsonArray.Clear();
        return this;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        if(Target is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else Clear();
    }

    /// <inheritdoc/>
    public IEnumerator<object?> GetEnumerator()
    {
        if (Target is IList list)
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
        else if (Target is Array array)
        {
            foreach (var item in array)
            {
                yield return item;
            }
        }
        else if (Target is IDictionary dictionary)
        {
            foreach (var item in dictionary.Values)
            {
                yield return item;
            }
        }
        else if (Target is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                yield return item;
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 比较
    /// </summary>
    /// <param name="onCompare"></param>
    public class Comparer(Func<Json, Json, int> onCompare) : IComparer
    {
        private Func<Json, Json, int> OnCompare { get; } = onCompare;

        /// <inheritdoc/>
        public int Compare(object? x, object? y)
        {
            return OnCompare(new Json(x), new Json(y));
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    /// <param name="onSort"></param>
    public void Sort(Func<Json, Json, int> onSort)
    {
        if(Target is List<object> listObjects)
        {
            listObjects.Sort((x, y) => onSort(new Json(x), new Json(y)));
        }
        else if (Target is IList list)
        {
            for(int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (onSort(new Json(list[i]), new Json(list[j])) > 0)
                    {
                        (list[j], list[i]) = (list[i], list[j]);
                    }
                }
            }
        }
        else if (Target is Array array)
        {
            Array.Sort(array, new Comparer(onSort));
        }
        else if (Target is JsonArray jsonArray)
        {
            throw new NotImplementedException();
        }
        else
        {
            throw new InvalidOperationException("ArrayWrapper: sort only support array type");
        }
    }

    /// <summary>
    /// Implicit convert json to array wrapper
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator ArrayWrapper(Json target)
    {
        return new ArrayWrapper(target.Node);
    }
}
