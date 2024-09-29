using System.Collections.Specialized;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;

namespace TidyHPC.Routers.Urls.Utils;

/// <summary>
/// 拓展
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="keys"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGet(this NameValueCollection self, string[] keys, out string value)
    {
        foreach (var key in keys)
        {
            if (self[key] != null)
            {
                value = self[key]!;
                return true;
            }
        }
        value = string.Empty;
        return false;
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="keys"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGet(this IDictionary<string,string> self, string[] keys, out string value)
    {
        foreach (var key in keys)
        {
            if (self.ContainsKey(key))
            {
                value = self[key];
                return true;
            }
        }
        value = string.Empty;
        return false;
    }

    /// <summary>
    /// 尝试获取值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="keys"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="onGetFailed"></param>
    /// <returns></returns>
    public static bool TryGet(this Json self, string[] keys, Type type, out object? value, Action onGetFailed)
    {
        if (self.IsNull)
        {
            value = null;
            return false;
        }
        foreach (var key in keys)
        {
            if (self.ContainsKey(key))
            {
                var temp = self[key];
                if (type == typeof(Json))
                {
                    value = temp;
                    return true;
                }
                else
                {
                    if (temp.ToString().TryConvertTo(type, out value))
                    {
                        return true;
                    }
                    else
                    {
                        onGetFailed();
                        value = null;
                        return false;
                    }
                }
            }
        }
        value = null;
        return false;
    }
}
