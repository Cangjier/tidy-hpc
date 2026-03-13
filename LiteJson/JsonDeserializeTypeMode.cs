namespace TidyHPC.LiteJson;

/// <summary>
/// Json 反序列化类型模式
/// </summary>
public enum JsonDeserializeTypeMode
{
    /// <summary>
    /// 字典和列表
    /// </summary>
    DictionaryAndList,
    /// <summary>
    /// System.Text.Json
    /// </summary>
    JsonElement
}