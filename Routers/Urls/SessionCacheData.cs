using System.Diagnostics.CodeAnalysis;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// 缓存数据
/// </summary>
public class SessionCacheData : IDisposable
{
    /// <summary>
    /// 缓存数据，如过滤期间对权限校验时获取的用户信息
    /// </summary>
    private Dictionary<string, object?> Data { get; set; } = new();

    /// <summary>
    /// 缓存数据，按类型获取
    /// </summary>
    private Dictionary<Type, object?> DataByType { get; set; } = new();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Data?.Clear();
        DataByType?.Clear();
        Data = null!;
        DataByType = null!;
    }

    /// <summary>
    /// 获取缓存数据，如果数据不存在则抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetOrThrow<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value is T t)
        {
            return t;
        }
        throw new Exception($"Cache data not found: {key}");
    }

    /// <summary>
    /// 获取缓存数据，如果数据不存在则抛出异常
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T GetOrThrow<T>()
    {
        if (DataByType.TryGetValue(typeof(T), out var value) && value is T t)
        {
            return t;
        }
        throw new Exception($"Cache data not found: {typeof(T)}");
    }

    /// <summary>
    /// 尝试获取缓存数据，按类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGet<T>(out T? value)
    {
        if (DataByType.TryGetValue(typeof(T), out object? valueObject) && valueObject is T t)
        {
            value = t;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// 尝试获取缓存数据，按类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGet(Type type, out object? value)
    {
        if (DataByType.TryGetValue(type, out value))
        {
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// 尝试获取缓存数据，按别名
    /// </summary>
    /// <param name="aliases"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGet(string[] aliases, out object? value)
    {
        foreach (var alias in aliases)
        {
            if (Data.TryGetValue(alias, out value))
            {
                return true;
            }
        }
        value = default;
        return false;
    }

    /// <summary>
    /// 设置缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Set<T>(string key, T value)
    {
        Data[key] = value;
    }

    /// <summary>
    /// 设置缓存数据，按类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public void Set<T>(T value)
    {
        DataByType[typeof(T)] = value;
    }

    /// <summary>
    /// 移除缓存数据，按类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Remove<T>()
    {
        DataByType.Remove(typeof(T));
    }

    /// <summary>
    /// 移除缓存数据，按键
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        Data.Remove(key);
    }

    /// <summary>
    /// 清空缓存数据
    /// </summary>
    public void Clear()
    {
        Data.Clear();
        DataByType.Clear();
    }
}