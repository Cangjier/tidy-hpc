namespace TidyHPC.LiteDB;

/// <summary>
/// 记录
/// </summary>
public interface IRecord
{
    /// <summary>
    /// 布局地址
    /// </summary>
    public long LayoutAddress { get; set; }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Read(byte[] buffer,int offset);

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    public void Write(byte[] buffer,int offset);
}
