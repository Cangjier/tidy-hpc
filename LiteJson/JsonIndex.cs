namespace TidyHPC.LiteJson;
/// <summary>
/// Json Index
/// </summary>
public struct JsonIndex
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public JsonIndex(string key)
    {
        Key = key;
        Index = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="index"></param>
    public JsonIndex(int index)
    {
        Key = null;
        Index = index;
    }

    /// <summary>
    /// Key, for Object
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// Index, for Array
    /// </summary>
    public int? Index { get; }

    /// <summary>
    /// implicit operator
    /// </summary>
    /// <param name="key"></param>
    public static implicit operator JsonIndex(string key)
    {
        return new JsonIndex(key);
    }

    /// <summary>
    /// implicit operator
    /// </summary>
    /// <param name="index"></param>
    public static implicit operator JsonIndex(int index)
    {
        return new JsonIndex(index);
    }

    /// <summary>
    /// Is Object
    /// </summary>
    public readonly bool IsObject => Key is not null;

    /// <summary>
    /// Is Array
    /// </summary>
    public readonly bool IsArray => Index is not null;

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Key ?? Index?.ToString() ?? string.Empty;
    }
}