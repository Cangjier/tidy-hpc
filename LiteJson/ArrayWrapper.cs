using System.Collections;
using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;

/// <summary>
/// Array Wrapper
/// </summary>
/// <param name="target"></param>
public struct ArrayWrapper(object? target) : IDisposable, IEnumerable<object?>
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
            if (Target is null) return 0;
            if (Target is IList list) return list.Count;
            if (Target is Array array) return array.Length;
            if (Target is IDictionary dictionary) return dictionary.Count;
            if (Target is JsonArray jsonArray) return jsonArray.Count;
            if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))) return Target.GetType().GetProperty("Count")?.GetValue(Target) as int? ?? 0;
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
            if (Target is null) return null;
            if (Target is IList list) return list[index];
            if (Target is Array array) return array.GetValue(index);
            if (Target is JsonArray jsonArray) return jsonArray[index];
            if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                return Target.GetType().GetProperty("Item")?.GetValue(Target, [index]);

            return null;
        }
        set
        {
            if (Target is null) return;
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
            else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
                Target.GetType().GetProperty("Item")?.SetValue(Target, value, [index]);
        }
    }

    /// <summary>
    /// 添加一个元素
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public ArrayWrapper Add(object? value)
    {
        if (Target is null) return this;
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
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            Target.GetType().GetMethod("Add")?.Invoke(Target, [value]);
        else throw new NotImplementedException();
        return this;
    }

    /// <summary>
    /// 插入一个元素
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Insert(int index, object? value)
    {
        if (Target is null) return;
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
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            Target.GetType().GetMethod("Insert")?.Invoke(Target, [index, value]);
        else throw new NotImplementedException();
    }

    /// <summary>
    /// 包含
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Contains(object? value)
    {
        if (Target is null) return false;
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
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            return Target.GetType().GetMethod("Contains")?.Invoke(Target, [value]) as bool? ?? false;
        return false;
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void RemoveAt(int index)
    {
        if (Target is null) return;
        if (Target is IList list) list.RemoveAt(index);
        else if (Target is Array array)
        {
            throw new NotImplementedException();
        }
        else if (Target is JsonArray jsonArray) jsonArray.RemoveAt(index);
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            Target.GetType().GetMethod("RemoveAt")?.Invoke(Target, [index]);
        else throw new NotImplementedException();
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
        if (Target is null) return this;
        if (Target is IList list) list.Clear();
        else if (Target is Array array) Array.Clear(array, 0, array.Length);
        else if (Target is IDictionary dictionary) dictionary.Clear();
        else if (Target is JsonArray jsonArray) jsonArray.Clear();
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
            Target.GetType().GetMethod("Clear")?.Invoke(Target, []);
        return this;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        if (Target is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else Clear();
    }

    /// <inheritdoc/>
    public IEnumerator<object?> GetEnumerator()
    {
        if (Target is null) yield break;
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
        else if (Target is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
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
        if (Target is null) return;
        if (Target is List<object> listObjects)
        {
            listObjects.Sort((x, y) => onSort(new Json(x), new Json(y)));
        }
        else if (Target is IList list)
        {
            for (int i = 0; i < list.Count; i++)
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
    /// 粘接
    /// </summary>
    /// <param name="start"></param>
    /// <param name="deleteCount"></param>
    /// <param name="items"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public List<object?> Splice(int start, int deleteCount, params Json[] items)
    {
        if (Target is null) return [];
        if (Target is List<object?> listObjects)
        {
            var nextCount = listObjects.Count - start;
            if (deleteCount > nextCount) deleteCount = nextCount;
            List<object?> result = listObjects.GetRange(start, deleteCount);
            listObjects.RemoveRange(start, deleteCount);
            listObjects.InsertRange(start, items.Select(item => item.Node));
            return result;
        }
        else if (Target is IList list)
        {
            var nextCount = list.Count - start;
            if (deleteCount > nextCount) deleteCount = nextCount;
            List<object?> result = [];
            for (int i = 0; i < deleteCount; i++)
            {
                result.Add(list[start]);
                list.RemoveAt(start);
            }
            for (int i = 0; i < items.Length; i++)
            {
                list.Insert(start + i, items[i].Node);
            }
            return result;
        }
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            var countMethod = Target.GetType().GetMethod("Count") ?? throw new Exception("Error code");
            var itemMethod = Target.GetType().GetMethod("Item") ?? throw new Exception("Error code");
            var addMethod = Target.GetType().GetMethod("Add") ?? throw new Exception("Error code");
            var insertMethod = Target.GetType().GetMethod("Insert") ?? throw new Exception("Error code");
            var removeAtMethod = Target.GetType().GetMethod("RemoveAt") ?? throw new Exception("Error code");
            var nextCount = (int)countMethod.Invoke(Target, [])! - start;
            if (deleteCount > nextCount) deleteCount = nextCount;
            List<object?> result = [];
            for (int i = 0; i < deleteCount; i++)
            {
                result.Add(itemMethod.Invoke(Target, [start])!);
                removeAtMethod.Invoke(Target, [start]);
            }
            for (int i = 0; i < items.Length; i++)
            {
                insertMethod.Invoke(Target, [start + i, items[i].Node]);
            }
            return result;
        }
        else
        {
            throw new InvalidOperationException($"ArrayWrapper: splice only support list type, {Target?.GetType()}");
        }
    }

    /// <summary>
    /// 切片
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<object?> Slice(int start, int end)
    {
        if (Target is null) return [];
        if (Target is List<object?> listObjects)
        {
            if (end >= listObjects.Count) end = listObjects.Count;
            return listObjects.GetRange(start, end - start);
        }
        else if (Target is IList list)
        {
            List<object?> result = [];
            var count = list.Count;
            for (int i = start; i < end && i < count; i++)
            {
                result.Add(list[i]);
            }
            return result;
        }
        else if (Target is Array array)
        {
            List<object?> result = [];
            for (int i = start; i < end && i < array.Length; i++)
            {
                result.Add(array.GetValue(i));
            }
            return result;
        }
        else if (Target is JsonArray jsonArray)
        {
            List<object?> result = [];
            for (int i = start; i < end && i < jsonArray.Count; i++)
            {
                result.Add(jsonArray[i]);
            }
            return result;
        }
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            var countMethod = Target.GetType().GetMethod("Count") ?? throw new Exception("Error code");
            var itemMethod = Target.GetType().GetMethod("Item") ?? throw new Exception("Error code");
            var nextCount = (int)countMethod.Invoke(Target, [])! - start;
            if (end > nextCount) end = nextCount;
            List<object?> result = [];
            for (int i = start; i < end && i < nextCount; i++)
            {
                result.Add(itemMethod.Invoke(Target, [i])!);
            }
            return result;
        }
        else
        {
            throw new InvalidOperationException("ArrayWrapper: slice only support list type");
        }
    }

    /// <summary>
    /// 反序
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<object?> Reverse()
    {
        if (Target is null) return [];
        if (Target is List<object?> listObjects)
        {
            listObjects.Reverse();
            return listObjects;
        }
        else if (Target is IList list)
        {
            List<object?> result = [];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                result.Add(list[i]);
            }
            return result;
        }
        else if (Target is Array array)
        {
            List<object?> result = [];
            for (int i = array.Length - 1; i >= 0; i--)
            {
                result.Add(array.GetValue(i));
            }
            return result;
        }
        else if (Target is JsonArray jsonArray)
        {
            List<object?> result = [];
            for (int i = jsonArray.Count - 1; i >= 0; i--)
            {
                result.Add(jsonArray[i]);
            }
            return result;
        }
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            var countMethod = Target.GetType().GetMethod("Count") ?? throw new Exception("Error code");
            var itemMethod = Target.GetType().GetMethod("Item") ?? throw new Exception("Error code");
            List<object?> result = [];
            for (int i = (int)countMethod.Invoke(Target, [])! - 1; i >= 0; i--)
            {
                result.Add(itemMethod.Invoke(Target, [i])!);
            }
            return result;
        }
        else
        {
            throw new InvalidOperationException("ArrayWrapper: reverse only support list type");
        }
    }

    /// <summary>
    /// 拼接数组
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public List<object?> Concat(params Json[] items)
    {
        List<object?> result = new();
        if (Target is null)
        {

        }
        else if (Target is List<object?> listObjects)
        {
            result.AddRange(listObjects);
        }
        else if (Target is IList list)
        {
            foreach (var item in list)
            {
                result.Add(item);
            }
        }
        else if (Target is Array array)
        {
            foreach (var item in array)
            {
                result.Add(item);
            }
        }
        else if (Target is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                result.Add(item);
            }
        }
        else if (Target.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)))
        {
            var countMethod = Target.GetType().GetMethod("Count") ?? throw new Exception("Error code");
            var itemMethod = Target.GetType().GetMethod("Item") ?? throw new Exception("Error code");
            for (int i = 0; i < (int)countMethod.Invoke(Target, [])!; i++)
            {
                result.Add(itemMethod.Invoke(Target, [i])!);
            }
        }
        else
        {
            throw new InvalidOperationException("ArrayWrapper: concat only support list type");
        }
        foreach (var item in items)
        {
            if (item.IsArray)
            {
                foreach (var subItem in item)
                {
                    result.Add(subItem.Node);
                }
            }
            else
            {
                result.Add(item.Node);
            }
        }
        return result;
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
