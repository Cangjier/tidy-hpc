namespace TidyHPC.Routers.Urls;

/// <summary>
/// Url方法
/// </summary>
public enum UrlMethods
{
    /// <summary>
    /// GET 方法请求一个指定资源的表示形式，使用 GET 的请求应该只被用于获取数据。
    /// </summary>
    HTTP_GET,
    /// <summary>
    /// POST 方法用于将实体提交到指定的资源，通常导致在服务器上的状态变化或副作用。
    /// </summary>
    HTTP_POST,
    /// <summary>
    /// PUT 方法用有效载荷请求替换目标资源的所有当前表示。
    /// </summary>
    HTTP_PUT,
    /// <summary>
    /// DELETE 方法删除指定的资源。
    /// </summary>
    HTTP_DELETE,
    /// <summary>
    /// PATCH 方法用于对资源应用部分修改。
    /// </summary>
    HTTP_PATCH,
    /// <summary>
    /// OPTIONS 方法用于描述目标资源的通信选项。
    /// </summary>
    HTTP_OPTIONS,
    /// <summary>
    /// HEAD 方法请求一个与 GET 请求的响应相同的响应，但没有响应体。
    /// </summary>
    HTTP_HEAD,
    /// <summary>
    /// CONNECT 方法建立一个到由目标资源标识的服务器的隧道。
    /// </summary>
    HTTP_CONNECT,
    /// <summary>
    /// TRACE 方法沿着到目标资源的路径执行一个消息环回测试。
    /// </summary>
    HTTP_TRACE,
    /// <summary>
    /// WebSocket 方法用于建立一个到服务器的WebSocket连接
    /// </summary>
    WebSocket,
    /// <summary>
    /// WebSocket_TextFrame 方法用于发送一个文本帧
    /// </summary>
    WebSocket_TextFrame,
    /// <summary>
    /// WebSocket_BinaryFrame 方法用于发送一个二进制帧
    /// </summary>
    WebSocket_BinaryFrame,

}

#region UrlMethod
/// <summary>
/// Url方法
/// </summary>
public class UrlMethodAttribute : Attribute
{
    /// <summary>
    /// 方法
    /// </summary>
    public UrlMethods Method { get; }

    /// <summary>
    /// Url方法
    /// </summary>
    /// <param name="method"></param>
    public UrlMethodAttribute(UrlMethods method)
    {
        Method = method;
    }
}

/// <summary>
/// Url Get 方法
/// </summary>
public class UrlGetAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Get 方法
    /// </summary>
    public UrlGetAttribute() : base(UrlMethods.HTTP_GET)
    {
    }
}

/// <summary>
/// Url Post 方法
/// </summary>
public class UrlPostAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Post 方法
    /// </summary>
    public UrlPostAttribute() : base(UrlMethods.HTTP_POST)
    {
    }
}

/// <summary>
/// Url Put 方法
/// </summary>
public class UrlPutAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Put 方法
    /// </summary>
    public UrlPutAttribute() : base(UrlMethods.HTTP_PUT)
    {
    }
}

/// <summary>
/// Url Delete 方法
/// </summary>
public class UrlDeleteAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Delete 方法
    /// </summary>
    public UrlDeleteAttribute() : base(UrlMethods.HTTP_DELETE)
    {
    }
}

/// <summary>
/// Url Patch 方法
/// </summary>
public class UrlPatchAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Patch 方法
    /// </summary>
    public UrlPatchAttribute() : base(UrlMethods.HTTP_PATCH)
    {
    }
}

/// <summary>
/// Url Head 方法
/// </summary>
public class UrlHeadAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Head 方法
    /// </summary>
    public UrlHeadAttribute() : base(UrlMethods.HTTP_HEAD)
    {
    }
}

/// <summary>
/// Url Options 方法
/// </summary>
public class UrlOptionsAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Options 方法
    /// </summary>
    public UrlOptionsAttribute() : base(UrlMethods.HTTP_OPTIONS)
    {
    }
}

/// <summary>
/// Url Trace 方法
/// </summary>
public class UrlTraceAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Trace 方法
    /// </summary>
    public UrlTraceAttribute() : base(UrlMethods.HTTP_TRACE)
    {

    }
}

/// <summary>
/// Url Connect 方法
/// </summary>
public class UrlConnectAttribute : UrlMethodAttribute
{
    /// <summary>
    /// Url Connect 方法
    /// </summary>
    public UrlConnectAttribute() : base(UrlMethods.HTTP_CONNECT)
    {

    }
}

#endregion
