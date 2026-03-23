using TidyHPC.LiteJson;

namespace TidyHPC.Extensions;

/// <summary>
/// String extensions
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert string to object of type
    /// </summary>
    /// <param name="from"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    public static object? ConvertTo(this string from, Type toType)
    {
        if (toType == typeof(string)) return from;
        else if (toType == typeof(int)) return int.Parse(from);
        else if (toType == typeof(long)) return long.Parse(from);
        else if (toType == typeof(float)) return float.Parse(from);
        else if (toType == typeof(double)) return double.Parse(from);
        else if (toType == typeof(bool)) return from == "true";
        else if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var list = Activator.CreateInstance(toType);
            var addMethod = toType.GetMethod("Add");
            using var array = Json.Parse(from);
            foreach (var item in array.GetArrayEnumerable())
            {
                addMethod!.Invoke(list, [item.ToString().ConvertTo(toType.GetGenericArguments()[0])]);
            }
            return list;
        }
        else if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var dict = Activator.CreateInstance(toType);
            var addMethod = toType.GetMethod("Add");
            using var array = Json.Parse(from);
            foreach (var item in array.GetObjectEnumerable())
            {
                addMethod!.Invoke(dict, [item.Key.ConvertTo(toType.GetGenericArguments()[0]), item.Value.ToString().ConvertTo(toType.GetGenericArguments()[1])]);
            }
            return dict;
        }
        else if (toType.IsEnum)
        {
            return Enum.Parse(toType, from);
        }
        else if (toType == typeof(DateTime))
        {
            return DateTime.Parse(from);
        }
        else if (toType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(from);
        }
        else if (toType == typeof(TimeSpan))
        {
            return TimeSpan.Parse(from);
        }
        else if (toType == typeof(Guid))
        {
            return Guid.Parse(from);
        }
        else if (toType == typeof(Uri))
        {
            return new Uri(from);
        }
        else if (toType == typeof(Version))
        {
            return new Version(from);
        }
        else if (toType == typeof(byte[]))
        {
            return Convert.FromBase64String(from);
        }
        else if (toType == typeof(char))
        {
            return from[0];
        }
        else if (toType == typeof(char[]))
        {
            return from.ToCharArray();
        }
        else if (toType == typeof(sbyte))
        {
            return sbyte.Parse(from);
        }
        else if (toType == typeof(short))
        {
            return short.Parse(from);
        }
        else if (toType == typeof(ushort))
        {
            return ushort.Parse(from);
        }
        else
        {
            return null;
        }
    }

    private static bool TryConvertTo_StringToNumber(this string from, Type toType, out object? to)
    {
        if (toType == typeof(int))
        {
            if (int.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(long))
        {
            if (long.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(float))
        {
            if (float.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(double))
        {
            if (double.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(sbyte))
        {
            if (sbyte.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(short))
        {
            if (short.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(ushort))
        {
            if (ushort.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
        }
        else if (toType == typeof(bool))
        {
            if (bool.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else if (from.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            {
                to = true;
                return true;
            }
            else if (from.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            {
                to = false;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        to = null;
        return false;
    }

    private static bool TryConvertTo_StringToBoolean(this string from, out object? to)
    {
        if (bool.TryParse(from, out var result))
        {
            to = result;
            return true;
        }
        else
        {
            to = null;
            return false;
        }
    }

    private static bool TryConvertTo_StringToDate(this string from, Type toType, out object? to)
    {
        if (toType == typeof(DateTime))
        {
            if (DateTime.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        if (toType == typeof(DateTimeOffset))
        {
            if (DateTimeOffset.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        else if (toType == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        else
        {
            to = null;
            return false;
        }
    }

    private static bool TryConvertTo_StringToOther(this string from, Type toType, out object? to)
    {
        if (toType == typeof(Guid))
        {
            if (Guid.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        else if (toType == typeof(Uri))
        {
            if (Uri.TryCreate(from, UriKind.RelativeOrAbsolute, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        else if (toType == typeof(Version))
        {
            if (Version.TryParse(from, out var result))
            {
                to = result;
                return true;
            }
            else
            {
                to = null;
                return false;
            }
        }
        else
        {
            to = null;
            return false;
        }
    }

    private static bool TryConvertTo_StringToArray(this string from, Type toType, out object? to)
    {
        if (toType == typeof(char[]))
        {
            to = from.ToCharArray();
            return true;
        }
        else
        {
            to = null;
            return false;
        }
    }

    private static bool TryConvertTo_StringToEnum(this string from, Type toType, out object? to)
    {
        if (toType.IsEnum)
        {
            return Enum.TryParse(toType, from, out to);
        }
        else
        {
            to = null;
            return false;
        }
    }

    /// <summary>
    /// Try convert string to object of type
    /// </summary>
    /// <param name="from"></param>
    /// <param name="toType"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static bool TryConvertTo(this string from, Type toType, out object? to)
    {
        if (toType == typeof(string))
        {
            to = from;
            return true;
        }
        else if (TryConvertTo_StringToNumber(from, toType, out to))
        {
            return true;
        }
        else if (TryConvertTo_StringToBoolean(from, out to))
        {
            return true;
        }
        else if (TryConvertTo_StringToDate(from, toType, out to))
        {
            return true;
        }
        else if (TryConvertTo_StringToArray(from, toType, out to))
        {
            return true;
        }
        else if (TryConvertTo_StringToOther(from, toType, out to))
        {
            return true;
        }
        else if (TryConvertTo_StringToEnum(from, toType, out to))
        {
            return true;
        }
        else
        {
            to = null;
            return false;
        }
    }

    /// <summary>
    /// Convert string to object of type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="from"></param>
    /// <returns></returns>
    public static T? ConvertTo<T>(this string from)
    {
        return (T?)from.ConvertTo(typeof(T));
    }
}
