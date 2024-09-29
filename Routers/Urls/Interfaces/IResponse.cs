namespace TidyHPC.Routers.Urls.Interfaces;

/// <summary>
/// 响应接口
/// </summary>
public interface IResponse:IDisposable
{
    /// <summary>
    /// 响应头
    /// </summary>
    IResponseHeaders Headers { get; }

    /// <summary>
    /// 响应体
    /// </summary>
    Stream Body { get; }

    /// <summary>
    /// 状态码
    /// </summary>
    int StatusCode { get; set; }
}
