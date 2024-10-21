namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 布局头部
/// </summary>
public class LayoutHeader<T>
    where T : LayoutHeader<T>
{
    /// <summary>
    /// 接口ID
    /// </summary>
    public Guid InterfaceID;

    /// <summary>
    /// 布局大小
    /// </summary>
    public int LayoutSize;

    /// <summary>
    /// 头大小
    /// </summary>
    public int HeaderSize;

    /// <summary>
    /// 记录大小
    /// </summary>
    public int RecordSize;
}
