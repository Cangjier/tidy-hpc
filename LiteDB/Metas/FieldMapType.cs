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
    /// 一对多映射，用于小量数据
    /// </summary>
    IndexArray,
    /// <summary>
    /// 一对多映射，用于中量数据
    /// </summary>
    IndexSmallHashSet,
    /// <summary>
    /// 一对多映射，用于大量数据
    /// </summary>
    IndexHashSet,
}
