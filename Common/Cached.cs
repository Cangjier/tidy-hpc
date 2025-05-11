namespace TidyHPC.Common;

/// <summary>
/// 缓存类
/// </summary>
/// <typeparam name="T"></typeparam>
public class Cached<T>
{
    private readonly Func<T> _getValue;
    private T? _value;
    private bool _isValueCreated = false;
    /// <summary>
    /// 缓存类
    /// </summary>
    /// <param name="getValue"></param>
    public Cached(Func<T> getValue)
    {
        _getValue = getValue;
    }

    /// <summary>
    /// 获取值
    /// </summary>
    public T Value
    {
        get
        {
            if (!_isValueCreated)
            {
                _value = _getValue();
                _isValueCreated = true;
            }
            return _value!;
        }
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="cached"></param>
    public static implicit operator T(Cached<T> cached)
    {
        return cached.Value;
    }


}