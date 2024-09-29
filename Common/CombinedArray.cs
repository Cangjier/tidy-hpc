namespace TidyHPC.Common;

/// <summary>
/// 合并数组
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Arrays"></param>
public record CombinedArray<T>(T[][] Arrays)
{
    /// <summary>
    /// Get the element by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public T this[int index]
    {
        get
        {
            var totalLength = 0;
            foreach (var array in Arrays)
            {
                if (index < totalLength + array.Length)
                {
                    return array[index - totalLength];
                }
                totalLength += array.Length;
            }
            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    /// The length of the combined array
    /// </summary>
    public int Length => Arrays.Sum(array => array.Length);
}
