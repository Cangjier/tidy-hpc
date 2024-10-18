namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 字段映射类型
/// </summary>
public enum FieldMapType:byte
{
    /// <summary>
    /// 普通字段
    /// </summary>
    None,
    /// <summary>
    /// 主键
    /// </summary>
    Master,
    /// <summary>
    /// 索引类型
    /// </summary>
    Index,
    /// <summary>
    /// 值可以重复的索引
    /// </summary>
    IndexHashSet,
}
