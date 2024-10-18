namespace TidyHPC.LiteDB.BasicValues;

/// <summary>
/// Hash Value
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IValue<T>
    where T : IValue<T>
{
    /// <summary>
    /// Write
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    int Write(byte[] buffer, int offset);
    /// <summary>
    /// Read
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    int Read(byte[] buffer, int offset);
    /// <summary>
    /// 设置为空
    /// </summary>
    void SetEmpty();
    /// <summary>
    /// 是否为空
    /// </summary>
    /// <returns></returns>
    bool IsEmpty();
    /// <summary>
    /// Get Size
    /// </summary>
    /// <returns></returns>
    static abstract int GetSize();
    /// <summary>
    /// Equals
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static virtual bool operator ==(T left, T right)
    {
        return left.Equals(right);
    }
    /// <summary>
    /// Unequals
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static virtual bool operator !=(T left, T right)
    {
        return !left.Equals(right);
    }
    /// <summary>
    /// GetHashCode
    /// </summary>
    /// <returns></returns>
    int GetHashCode();
    /// <summary>
    /// Equals
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    bool Equals(object? obj);

    /// <summary>
    /// 获取固定HashCode
    /// </summary>
    /// <returns></returns>
    Task<ulong> GetHashCode(Database database);
}