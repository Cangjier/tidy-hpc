namespace TidyHPC.Locks;

/// <summary>
/// 锁定字符串
/// </summary>
public class LockString : Lock<string>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockString(string value) : base(value)
    {
    }

    /// <summary>
    /// 隐式转换为LockString
    /// </summary>
    /// <param name="self"></param>
    public static implicit operator LockString(string self) => new(self);

    /// <summary>
    /// 隐式转换为string
    /// </summary>
    /// <param name="self"></param>
    public static implicit operator string(LockString self) => self.Value;

    /// <summary>
    /// 字符串连接
    /// </summary>
    /// <param name="self"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    public static LockString operator +(LockString self, string str)
    {
        self.Process(self => self + str);
        return self;
    }

    /// <summary>
    /// 索引器
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public char this[int index] => Process(self => self[index]);

    /// <summary>
    /// 长度
    /// </summary>
    public int Length => Process(self => self.Length);
}

