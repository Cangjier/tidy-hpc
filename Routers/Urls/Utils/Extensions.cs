using System.Collections.Specialized;
using System.Text.Json;
using TidyHPC.Extensions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;

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
    public static bool TryGet(this IDictionary<string, string> self, string[] keys, out string value)
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
        try
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
                    else if (temp.Node is JsonElement jsonElement)
                    {
                        value = jsonElement.Deserialize(type);
                        return true;
                    }
                    else if (type == typeof(string[]))
                    {
                        value = temp.ToArray(item => item.AsString);
                        return true;
                    }
                    else if (type == typeof(int[]))
                    {
                        value = temp.ToArray(item => item.ToInt32);
                        return true;
                    }
                    else if (type == typeof(float[]))
                    {
                        value = temp.ToArray(item => item.ToFloat);
                        return true;
                    }
                    else if (type == typeof(double[]))
                    {
                        value = temp.ToArray(item => item.ToDouble);
                        return true;
                    }
                    else if (type == typeof(int) && temp.IsInt32)
                    {
                        value = temp.AsInt32;
                        return true;
                    }
                    else if (type == typeof(float) && temp.IsFloat)
                    {
                        value = temp.AsFloat;
                        return true;
                    }
                    else if (type == typeof(double) && temp.IsDouble)
                    {
                        value = temp.AsDouble;
                        return true;
                    }
                    else if (type == typeof(bool) && temp.IsBoolean)
                    {
                        value = temp.AsBoolean;
                        return true;
                    }
                    else if (type == typeof(Guid) && temp.IsGuid)
                    {
                        value = temp.AsGuid;
                        return true;
                    }
                    else if (type == typeof(DateTime) && temp.IsDateTime)
                    {
                        value = temp.AsDateTime;
                        return true;
                    }
                    else if (type == typeof(TimeSpan) && temp.IsTimeSpan)
                    {
                        value = temp.AsTimeSpan;
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
            value = null;
            return false;
        }
        catch(Exception e)
        {
            Logger.Error(e);
            onGetFailed();
            value = null;
            return false;
        }
    }
}
