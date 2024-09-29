using System.Collections;

namespace TidyHPC.LiteJson;
/// <summary>
/// Json Path
/// </summary>
public readonly struct JsonPath : IEnumerable<JsonIndex>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="target"></param>
    public JsonPath(params JsonIndex[] target)
    {
        Target = target;
    }

    /// <summary>
    /// 封装对象
    /// </summary>
    public JsonIndex[] Target { get; }

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns></returns>
    public IEnumerator<JsonIndex> GetEnumerator()
    {
        return ((IEnumerable<JsonIndex>)Target).GetEnumerator();
    }

    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<JsonIndex>)Target).GetEnumerator();
    }

    /// <summary>
    /// 第一个
    /// </summary>
    public JsonIndex First => Target[0];

    /// <summary>
    /// 下一个
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public JsonPath Next(JsonIndex index)
    {
        if (Target is null) return new JsonPath([index]);
        else
        {
            var target = new JsonIndex[Target.Length + 1];
            Target.CopyTo(target, 0);
            target[Target.Length] = index;
            return new JsonPath(target);
        }
    }
}
