namespace TidyHPC.LiteDB.Layouts.Visitors;

/// <summary>
/// 统计区域的访问器
/// </summary>
/// <param name="block"></param>
public class StatisticalRegionVisitor(StatisticalLayout block)
{
    /// <summary>
    /// 访问的块
    /// </summary>
    public StatisticalLayout Block { get; } = block;

    
}