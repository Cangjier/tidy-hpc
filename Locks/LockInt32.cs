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
    /// <param name="self"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static LockInt32 operator +(LockInt32 self, int add)
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
    public static LockInt32 operator -(LockInt32 self, int sub)
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
    public static LockInt32 operator *(LockInt32 self, int mul)
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
    public static LockInt32 operator /(LockInt32 self, int div)
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
    public static LockInt32 operator %(LockInt32 self, int mod)
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
    public static LockInt32 operator &(LockInt32 self, int and)
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
    public static LockInt32 operator |(LockInt32 self, int or)
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
    public static LockInt32 operator ^(LockInt32 self, int xor)
    {
        self.Process(value => value ^ xor);
        return self;
    }

    /// <summary>
    /// 按位非
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt32 operator ~(LockInt32 self)
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
    public static LockInt32 operator <<(LockInt32 self, int shift)
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
    public static LockInt32 operator >>(LockInt32 self, int shift)
    {
        self.Process(value => value >> shift);
        return self;
    }

    /// <summary>
    /// 正数
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt32 operator +(LockInt32 self)
    {
        self.Process(value => +value);
        return self;
    }

    /// <summary>
    /// 负数
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt32 operator -(LockInt32 self)
    {
        self.Process(value => -value);
        return self;
    }
    
    /// <summary>
    /// 自增
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt32 operator ++(LockInt32 self)
    {
        self.Process(value => value + 1);
        return self;
    }

    /// <summary>
    /// 自减
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static LockInt32 operator --(LockInt32 self)
    {
        self.Process(value => value - 1);
        return self;
    }
}