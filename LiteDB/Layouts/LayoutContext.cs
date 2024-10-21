using TidyHPC.Queues;

namespace TidyHPC.LiteDB.Layouts;

/// <summary>
/// 布局上下文
/// </summary>
public class LayoutContext
{
    /// <summary>
    /// 布局上下文
    /// </summary>
    /// <param name="fileStream"></param>
    public LayoutContext(FileStreamQueue fileStream)
    {
        FileStream = fileStream;
    }

    /// <summary>
    /// 文件流队列
    /// </summary>
    public FileStreamQueue FileStream { get; set; }
}
