using System.Text;

namespace TidyHPC.Extensions;

/// <summary>
/// IEnumerable Extensions
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Foreach Extension
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static void Foreach<T>(this IEnumerable<T> self, Action<T> action)
    {
        foreach (var item in self)
        {
            action(item);
        }
    }

    /// <summary>
    /// Foreach Extension
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static void Foreach<T>(this IEnumerable<T> self, Action<T, int> action)
    {
        int index = 0;
        foreach (var item in self)
        {
            action(item, index);
            index++;
        }
    }

    /// <summary>
    /// Join the IEnumerable with splitChar
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="splitChar"></param>
    /// <param name="appendEnd"></param>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> self, string splitChar, bool appendEnd = false)
    {
        StringBuilder temp = new();
        bool isFirst = true;
        foreach (var i in self)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                temp.Append(splitChar);
            }
            temp.Append(i);
        }
        if (appendEnd)
        {
            temp.Append(splitChar);
        }
        return temp.ToString();
    }

    /// <summary>
    /// Join the IEnumerable with splitChar
    /// </summary>
    /// <param name="self"></param>
    /// <param name="splitChar"></param>
    /// <param name="appendEnd"></param>
    /// <param name="onPair"></param>
    /// <returns></returns>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public static string Join<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> self, string splitChar,Func<TKey,TValue,object> onPair, bool appendEnd = false)
        where TKey : notnull
    {
        StringBuilder temp = new();
        bool isFirst = true;
        foreach (var i in self)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                temp.Append(splitChar);
            }
            temp.Append(onPair(i.Key, i.Value));
        }
        if (appendEnd)
        {
            temp.Append(splitChar);
        }
        return temp.ToString();
    }

    /// <summary>
    /// Join the IEnumerable with splitChar and itemAction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="splitChar"></param>
    /// <param name="itemAction"></param>
    /// <param name="appendEnd"></param>
    /// <returns></returns>
    public static string Join<T>(this IEnumerable<T> self, string splitChar, Func<T, object> itemAction, bool appendEnd = false)
    {
        StringBuilder temp = new StringBuilder();
        bool isFirst = true;
        foreach (var i in self)
        {
            if (isFirst)
            {
                isFirst = false;
            }
            else
            {
                temp.Append(splitChar);
            }
            temp.Append(itemAction(i));
        }
        if (appendEnd)
        {
            temp.Append(splitChar);
        }
        return temp.ToString();
    }

    /// <summary>
    /// Foreach Extension with async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task ForeachAsync<T>(this IEnumerable<T> self, Func<T, Task> action)
    {
        foreach (var item in self)
        {
            await action(item);
        }
    }

    /// <summary>
    /// Foreach Extension with async, break when action return true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static bool Foreach<T>(this IEnumerable<T> self, Func<T, bool> action)
    {
        foreach (var item in self)
        {
            if (action(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Find the first item that match the action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static T? Find<T>(this IEnumerable<T> self, Func<T, bool> action)
    {
        foreach (var item in self)
        {
            if (action(item))
            {
                return item;
            }
        }
        return default;
    }

    /// <summary>
    /// Find the first item that match the action with async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task<T?> FindAsync<T>(this IEnumerable<T> self, Func<T, Task<bool>> action)
    {
        foreach (var item in self)
        {
            if (await action(item))
            {
                return item;
            }
        }
        return default;
    }

}
