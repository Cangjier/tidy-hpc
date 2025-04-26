namespace TidyHPC.Locks;

/// <summary>
/// 锁定int64
/// </summary>
public class LockInt64 : Lock<long>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="value"></param>
    public LockInt64(long value) : base(value)
    {
    }

    /// <summary>
    /// 隐式转换为LockInt64
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator LockInt64(long value) => new(value);

    /// <summary>
    /// 隐式转换为long
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator long(LockInt64 value) => value.Value;

    /// <summary>
    /// 加法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static LockInt64 operator +(LockInt64 self, long add)
    {
        self.Process(value => value + add);
        return self;
    }

    /// <summary>
    /// 减法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="sub"></param>
    /// <returns></returns>
    public static LockInt64 operator -(LockInt64 self, long sub)
    {
        self.Process(value => value - sub);
        return self;
    }

    /// <summary>
    /// 乘法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="mul"></param>
    /// <returns></returns>
    public static LockInt64 operator *(LockInt64 self, long mul)
    {
        self.Process(value => value * mul);
        return self;
    }

    /// <summary>
    /// 除法
    /// </summary>
    /// <param name="self"></param>
    /// <param name="div"></param>
    /// <returns></returns>
    public static LockInt64 operator /(LockInt64 self, long div)
    {
        self.Process(value => value / div);
        return self;
    }
    
    /// <summary>
    /// 取模
    /// </summary>
    /// <param name="self"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    public static LockInt64 operator %(LockInt64 self, long mod)
    {
        self.Process(value => value % mod);
        return self;
    }

    /// <summary>
    /// 按位与
    /// </summary>
    /// <param name="self"></param>
    /// <param name="and"></param>
    /// <returns></returns>
    public static LockInt64 operator &(LockInt64 self, long and)
    {
        self.Process(value => value & and);
        return self;
    }

    /// <summary>
    /// 按位或
    /// </summary>
    /// <param name="self"></param>
    /// <param name="or"></param>
    /// <returns></returns>
    public static LockInt64 operator |(LockInt64 self, long or)
    {
        self.Process(value => value | or);
        return self;
    }

    /// <summary>
    /// 按位异或
    /// </summary>
    /// <param name="self"></param>
    /// <param name="xor"></param>
    /// <returns></returns>
    public static LockInt64 operator ^(LockInt64 self, long xor)
    {
        self.Process(value => value ^ xor);
        return self;
    }

    /// <summary>
    /// 按位非
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt64 operator ~(LockInt64 self)
    {
        self.Process(value => ~value);
        return self;
    }

    /// <summary>
    /// 左移
    /// </summary>
    /// <param name="self"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public static LockInt64 operator <<(LockInt64 self, int shift)
    {
        self.Process(value => value << shift);
        return self;
    }

    /// <summary>
    /// 右移
    /// </summary>
    /// <param name="self"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public static LockInt64 operator >>(LockInt64 self, int shift)
    {
        self.Process(value => value >> shift);
        return self;
    }

    /// <summary>
    /// 正数
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt64 operator +(LockInt64 self)
    {
        self.Process(value => +value);
        return self;
    }

    /// <summary>
    /// 负数
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt64 operator -(LockInt64 self)
    {
        self.Process(value => -value);
        return self;
    }

    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt64 operator ++(LockInt64 self)
    {
        self.Process(value => value + 1);
        return self;
    }

    /// <summary>
    /// 自减
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt64 operator --(LockInt64 self)
    {
        self.Process(value => value - 1);
        return self;
    }

    /// <summary>
    /// 加法(int)
    /// </summary>
    /// <param name="self"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static LockInt64 operator +(LockInt64 self, int add)
    {
        self.Process(value => value + add);
        return self;
    }

    /// <summary>
    /// 减法(int)
    /// </summary>
    /// <param name="self"></param>
    /// <param name="sub"></param>
    /// <returns></returns>
    public static LockInt64 operator -(LockInt64 self, int sub)
    {
        self.Process(value => value - sub);
        return self;
    }
    

}
