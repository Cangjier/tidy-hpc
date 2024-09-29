namespace TidyHPC.Routers.Urls.Exceptions;

/// <summary>
/// 没有路由异常
/// </summary>
public class NoRouterException : Exception
{
    /// <summary>
    /// Url
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// No Router Exception
    /// </summary>
    /// <param name="url"></param>
    public NoRouterException(string url)
    {
        Url = url;
    }
}
