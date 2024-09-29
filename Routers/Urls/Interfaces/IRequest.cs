namespace TidyHPC.Routers.Urls.Interfaces;

/// <summary>
/// 请求接口
/// </summary>
public interface IRequest:IDisposable
{
    /// <summary>
    /// 请求地址
    /// </summary>
    Uri? Url { get; }

    /// <summary>
    /// 查询参数
    /// </summary>
    IDictionary<string,string> Query { get; }

    /// <summary>
    /// 头信息
    /// </summary>
    IRequestHeaders Headers { get; }

    /// <summary>
    /// 当前请求的方法
    /// </summary>
    UrlMethods Method { get; }

    /// <summary>
    /// 请求体的流
    /// </summary>
    Stream Body { get; }
}
