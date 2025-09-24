using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyHPC.Extensions;

/// <summary>
/// Extensions for ICollect interface
/// </summary>
public static class ICollectExtensions
{
#if NET6_0
    /// <summary>
    /// Orders the collection in place based on the provided key selector.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollection<T> Order<T>(this ICollection<T> collection, Func<T, object> keySelector)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        var ordered = collection.OrderBy(keySelector).ToList();
        collection.Clear();
        foreach (var item in ordered)
        {
            collection.Add(item);
        }
        return collection;
    }

    /// <summary>
    /// Orders the collection in place using the default comparer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ICollection<T> Order<T>(this ICollection<T> collection)
    {
        if (collection == null) throw new ArgumentNullException(nameof(collection));
        var ordered = collection.OrderBy(x => x).ToList();
        collection.Clear();
        foreach (var item in ordered)
        {
            collection.Add(item);
        }
        return collection;
    }
#endif
}
