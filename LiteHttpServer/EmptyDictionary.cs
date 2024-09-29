using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace TidyHPC.LiteHttpServer;

/// <summary>
/// 空字典
/// </summary>
public readonly struct EmptyDictionary : IDictionary<string, string>
{
    /// <summary>
    /// 索引
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string this[string key]
    {
        get => string.Empty;
        set { }
    }

    /// <summary>
    /// 键名
    /// </summary>
    public ICollection<string> Keys => Array.Empty<string>();

    /// <summary>
    /// 键值
    /// </summary>
    public ICollection<string> Values => Array.Empty<string>();

    /// <summary>
    /// 数量
    /// </summary>
    public int Count => 0;

    /// <summary>
    /// 是否只读
    /// </summary>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(string key, string value)
    {

    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, string> item)
    {

    }

    /// <inheritdoc/>
    public void Clear()
    {

    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, string> item)
    {
        return false;
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return false;
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {

    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var item in Array.Empty<KeyValuePair<string, string>>())
        {
            yield return item;
        }
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        return false;
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, string> item)
    {
        return false;
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        value = string.Empty;
        return false;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
