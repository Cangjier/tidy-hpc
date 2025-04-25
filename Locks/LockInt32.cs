namespace TidyHPC.Locks;

/// <summary>
/// 锁定int32
/// </summary>
public class LockInt32 : Lock<int>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockInt32(int value) : base(value)
    {
    }
    
    /// <summary>
    /// 隐式转换为LockInt32
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LockInt32(int value) => new(value);

    /// <summary>
    /// 隐式转换为int
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator int(LockInt32 value) => value.Value;
    
    /// <summary>
    /// 加法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static LockInt32 operator +(LockInt32 value, int add) => new(value.Value + add);

    /// <summary>
    /// 减法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="sub"></param>
    /// <returns></returns>
    public static LockInt32 operator -(LockInt32 value, int sub) => new(value.Value - sub);

    /// <summary>
    /// 乘法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mul"></param>
    /// <returns></returns>
    public static LockInt32 operator *(LockInt32 value, int mul) => new(value.Value * mul);

    /// <summary>
    /// 除法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="div"></param>
    /// <returns></returns>
    public static LockInt32 operator /(LockInt32 value, int div) => new(value.Value / div);

    /// <summary>
    /// 取模
    /// </summary>
    /// <param name="value"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    public static LockInt32 operator %(LockInt32 value, int mod) => new(value.Value % mod);

    /// <summary>
    /// 按位与
    /// </summary>
    /// <param name="value"></param>
    /// <param name="and"></param>
    /// <returns></returns>
    public static LockInt32 operator &(LockInt32 value, int and) => new(value.Value & and);

    /// <summary>
    /// 按位或
    /// </summary>
    /// <param name="value"></param>
    /// <param name="or"></param>
    /// <returns></returns>
    public static LockInt32 operator |(LockInt32 value, int or) => new(value.Value | or);

    /// <summary>
    /// 按位异或
    /// </summary>
    /// <param name="value"></param>
    /// <param name="xor"></param>
    /// <returns></returns>
    public static LockInt32 operator ^(LockInt32 value, int xor) => new(value.Value ^ xor);

    /// <summary>
    /// 按位非
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockInt32 operator ~(LockInt32 value) => new(~value.Value);

    /// <summary>
    /// 左移
    /// </summary>
    /// <param name="value"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public static LockInt32 operator <<(LockInt32 value, int shift) => new(value.Value << shift);

    /// <summary>
    /// 右移
    /// </summary>
    /// <param name="value"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public static LockInt32 operator >>(LockInt32 value, int shift) => new(value.Value >> shift);

    /// <summary>
    /// 正数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockInt32 operator +(LockInt32 value) => new(+value.Value);

    /// <summary>
    /// 负数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockInt32 operator -(LockInt32 value) => new(-value.Value);
    
    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockInt32 operator ++(LockInt32 value) => new(value.Value + 1);

    /// <summary>
    /// 自减
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static LockInt32 operator --(LockInt32 value) => new(value.Value - 1);
    
    
    
    
    
    
    
    
}