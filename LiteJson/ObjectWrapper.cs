using System.Collections;
using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;

/// <summary>
/// Object Wrapper
/// </summary>
/// <param name="target"></param>
public struct ObjectWrapper(object? target):IDisposable,IEnumerable<KeyValuePair<string,object?>>
{
    /// <summary>
    /// The target object
    /// </summary>
    public object? Target { get; } = target;

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public readonly object? this[string key]
    {
        get
        {
            if (Target is JsonObject jsonObject)
            {
                return jsonObject[key];
            }
            else if (Target is IDictionary dictionary)
            {
                return dictionary[key];
            }
            else if(Target is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                var type = Target.GetType();
                var members = type.GetMember(key);
                if (members.Length == 0) throw new KeyNotFoundException(key);
                var fieldOrProperty = members.Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property).FirstOrDefault();
                if (fieldOrProperty == null) throw new KeyNotFoundException(key);
                if (fieldOrProperty.MemberType == System.Reflection.MemberTypes.Field)
                {
                    return ((System.Reflection.FieldInfo)fieldOrProperty).GetValue(Target);
                }
                else if (fieldOrProperty.MemberType == System.Reflection.MemberTypes.Property)
                {
                    return ((System.Reflection.PropertyInfo)fieldOrProperty).GetValue(Target);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
        set
        {
            if(value is Json json)
            {
                value = json.Node;
            }
            if (Target is JsonObject jsonObject)
            {
                jsonObject[key] = value.ToJsonNode();
            }
            else if (Target is IDictionary dictionary)
            {
                dictionary[key] = value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// 获取所有的键
    /// </summary>
    public string[] Keys
    {
        get
        {
            if(Target is JsonObject jsonObject)
            {
                return jsonObject.Select(i => i.Key).ToArray();
            }
            else if(Target is IDictionary dictionary)return dictionary.Keys.Cast<string>().ToArray();
            else if(Target is null)throw new NullReferenceException();
            else
            {
                var fieldsOrProperties = Target.GetType().GetMembers().Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property);
                return fieldsOrProperties.Select(i => i.Name).ToArray();
            }
        }
    }

    /// <summary>
    /// 获取所有的值
    /// </summary>
    public object?[] Values
    {
        get
        {
            if (Target is JsonObject jsonObject)
            {
                return jsonObject.Select(i => i.Value).ToArray();
            }
            else if (Target is IDictionary dictionary) return [.. dictionary.Values];
            else if(Target is null)throw new NullReferenceException();
            else
            {
                var fieldsOrProperties = Target.GetType().GetMembers().Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property);
                var target = Target;
                return fieldsOrProperties.Select(i =>
                {
                    if (i.MemberType == System.Reflection.MemberTypes.Field)
                    {
                        return ((System.Reflection.FieldInfo)i).GetValue(target);
                    }
                    else if (i.MemberType == System.Reflection.MemberTypes.Property)
                    {
                        return ((System.Reflection.PropertyInfo)i).GetValue(target);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }).ToArray();
            }
        }
    }

    /// <summary>
    /// 数量
    /// </summary>
    public int Count
    {
        get
        {
            if (Target is JsonObject jsonObject)
            {
                return jsonObject.Count;
            }
            else if (Target is IDictionary dictionary)
            {
                return dictionary.Count;
            }
            else if (Target is null)
            {
                throw new NullReferenceException();
            }
            else
            {
                return Target.GetType().GetMembers().Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property).Count();
            }
        }
    }

    /// <summary>
    /// Contains key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool ContainsKey(string key)
    {
        if (Target is JsonObject jsonObject)
        {
            return jsonObject.ContainsKey(key);
        }
        else if (Target is IDictionary dictionary)
        {
            return dictionary.Contains(key);
        }
        else if(Target is null)
        {
            throw new NullReferenceException();
        }
        else
        {
            var type = Target.GetType();
            var members = type.GetMember(key);
            if (members.Length == 0) return false;
            var fieldOrProperty = members.Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property).FirstOrDefault();
            if (fieldOrProperty == null) return false;
            return true;
        }
    }

    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Remove(string key)
    {
        if (Target is JsonObject jsonObject)
        {
            jsonObject.Remove(key);
        }
        else if (Target is IDictionary dictionary)
        {
            dictionary.Remove(key);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Add
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Add(string key, object? value)
    {
        if (Target is JsonObject jsonObject)
        {
            jsonObject.Add(key, value.ToJsonNode());
        }
        else if (Target is IDictionary dictionary)
        {
            dictionary.Add(key, value);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Clear the object
    /// </summary>
    public void Clear()
    {
        if (Target is JsonObject jsonObject)
        {
            jsonObject.Clear();
        }
        else if (Target is IDictionary dictionary)
        {
            dictionary.Clear();
        }
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
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        if (Target is IDictionary dictionary)
        {
            foreach (DictionaryEntry item in dictionary)
            {
                yield return new KeyValuePair<string, object?>((string)item.Key, item.Value);
            }
        }
        else if (Target is JsonObject jsonObject)
        {
            foreach (var item in jsonObject)
            {
                yield return new KeyValuePair<string, object?>(item.Key, item.Value);
            }
        }
        else if (Target is null) throw new NullReferenceException();
        else
        {
            var fieldsOrProperties = Target.GetType().GetMembers().Where(i => i.MemberType == System.Reflection.MemberTypes.Field || i.MemberType == System.Reflection.MemberTypes.Property);
            var target = Target;
            foreach (var i in fieldsOrProperties)
            {
                if (i.MemberType == System.Reflection.MemberTypes.Field)
                {
                    yield return new KeyValuePair<string, object?>(i.Name, ((System.Reflection.FieldInfo)i).GetValue(target));
                }
                else if (i.MemberType == System.Reflection.MemberTypes.Property)
                {
                    yield return new KeyValuePair<string, object?>(i.Name, ((System.Reflection.PropertyInfo)i).GetValue(target));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Implicit convert json to object wrapper
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator ObjectWrapper(Json target)
    {
        return new ObjectWrapper(target.Node);
    }

}
