namespace TidyHPC.LiteDB.BasicValues;

/// <summary>
/// 整数
/// </summary>
public interface IInterger
{
    /// <summary>
    /// 获取数字的值
    /// </summary>
    /// <returns></returns>
    static abstract int GetValue();
}

/// <summary>
/// 数字1
/// </summary>
public struct Interger_1 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 1;
    }
}

/// <inheritdoc/>
public struct Interger_2 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 2;
    }
}

/// <inheritdoc/>
public struct Interger_4 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 4;
    }
}

/// <inheritdoc/>
public struct Interger_8 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 8;
    }
}

/// <inheritdoc/>
public struct Interger_16 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 16;
    }
}

/// <inheritdoc/>
public struct Interger_32 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 32;
    }
}

/// <inheritdoc/>
public struct Interger_64 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 64;
    }
}

/// <inheritdoc/>
public struct Interger_128 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 128;
    }
}

/// <inheritdoc/>
public struct Interger_256 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 256;
    }
}

/// <inheritdoc/>
public struct Interger_512 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 512;
    }
}

/// <inheritdoc/>
public struct Interger_1024 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 1024;
    }
}

/// <inheritdoc/>
public struct Interger_2048 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 2048;
    }
}

/// <inheritdoc/>
public struct Interger_4096 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 4096;
    }
}

/// <inheritdoc/>
public struct Interger_8192 : IInterger
{
    /// <inheritdoc/>
    public static int GetValue()
    {
        return 8192;
    }
}

