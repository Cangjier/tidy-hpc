namespace TidyHPC.Locks;

/// <summary>
/// 锁定布尔值
/// </summary>
public class LockBoolean : Lock<bool>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockBoolean(bool value) : base(value)
    {
    }

    /// <summary>
    /// 隐式转换为LockBoolean
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LockBoolean(bool value) => new(value);

    /// <summary>
    /// 隐式转换为bool
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator bool(LockBoolean value) => value.Value;

    /// <summary>
    /// 逻辑与
    /// </summary>
    /// <param name="self"></param>
    /// <param name="and"></param>
    /// <returns></returns>
    public static LockBoolean operator &(LockBoolean self, bool and)
    {
        self.Process(value => value & and);
        return self;
    }

    /// <summary>
    /// 逻辑或
    /// </summary>
    /// <param name="self"></param>
    /// <param name="or"></param>
    /// <returns></returns>
    public static LockBoolean operator |(LockBoolean self, bool or)
    {
        self.Process(value => value | or);
        return self;
    }

    /// <summary>
    /// 逻辑异或
    /// </summary>
    /// <param name="self"></param>
    /// <param name="xor"></param>
    /// <returns></returns>
    public static LockBoolean operator ^(LockBoolean self, bool xor)
    {
        self.Process(value => value ^ xor);
        return self;
    }

    /// <summary>
    /// 逻辑非
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockBoolean operator !(LockBoolean self)
    {
        self.Process(value => !value);
        return self;
    }
}
