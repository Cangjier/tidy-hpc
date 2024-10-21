using TidyHPC.LiteDB.Layouts.Visitors;

namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 基础Block
/// </summary>
public abstract class Layout
{
    /// <summary>
    /// Constructor
    /// </summary>
    public Layout()
    {
        RecordVisitor = new RecordVisitor(this);
        RecordRegionVisitor = new(this);
    }

    /// <summary>
    /// 布局配置
    /// </summary>
    public LayoutProperties Properties { get; private set; } = new();

    /// <summary>
    /// 记录访问器
    /// </summary>
    public RecordVisitor RecordVisitor { get; }

    /// <summary>
    /// 记录区域的访问器
    /// </summary>
    public RecordRegionVisitor RecordRegionVisitor { get; }
}

