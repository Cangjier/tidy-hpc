namespace TidyHPC.Common;
/// <summary>
/// 引用
/// </summary>
public class Ref<T>(T value)
{
    /// <summary>
    /// 值
    /// </summary>
    public T Value = value;
}
