namespace TidyHPC.LiteDB;

/// <summary>
/// 记录的索引
/// </summary>
public struct RecordIndex
{
    /// <summary>
    /// 记录的地址
    /// </summary>
    public long Address;

    /// <summary>
    /// 记录的主键值
    /// </summary>
    public Guid Master;

    /// <summary>
    /// 转换成 Json 字符串
    /// </summary>
    /// <returns></returns>
    public readonly override string ToString()
    {
        return $$"""
            {
                ""Address"":{{Address}},
                ""Master"":""{{Master}}""
            }
            """;
    }
}
