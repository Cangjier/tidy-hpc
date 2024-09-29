namespace TidyHPC.LiteDB;

/// <summary>
/// 记录
/// </summary>
internal interface IRecord
{
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
