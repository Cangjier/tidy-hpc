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
    /// <param name="value"></param>
    /// <param name="and"></param>
    /// <returns></returns>
    public static LockBoolean operator &(LockBoolean value, bool and) => new(value.Value & and);

    /// <summary>
    /// 逻辑或
    /// </summary>
    /// <param name="value"></param>
    /// <param name="or"></param>
    /// <returns></returns>
    public static LockBoolean operator |(LockBoolean value, bool or) => new(value.Value | or);

    /// <summary>
    /// 逻辑异或
    /// </summary>
    /// <param name="value"></param>
    /// <param name="xor"></param>
    /// <returns></returns>
    public static LockBoolean operator ^(LockBoolean value, bool xor) => new(value.Value ^ xor);

    /// <summary>
    /// 逻辑非
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockBoolean operator !(LockBoolean value) => new(!value.Value);
}

