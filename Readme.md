# TidyHPC
- LiteJson: 和System.Text.Json相比，具备更强的兼容性，内部对象既可以是System.Text.Json，也可以是List/Dictionay等Object。
- LiteDB: 类似于IndexDB，轻量级数据库
- LiteHttpServer: 基于HttpListener封装的服务器，主要配合UrlRouter使用
- UrlRouter: Url路由，只需要提供Session，即可路由，支持Filter/Router。

## UrlRouter
### Create
```csharp
UrlRouter urlRouter=new();
```
### No Router
```csharp
urlRouter.Events.HandleNoRoute = async (url, session) =>
{
    await Task.CompletedTask;
    session.Complete(() =>
    {
        Logger.InfoParameter("404", url);
        session.Response.StatusCode = 404;
        session.Response.Headers.SetHeader("Content-Type", "text/html");
        session.Response.Body.Write(Util.UTF8.GetBytes($"""
    <html>
    <head>
    <title>404 Not Found</title>
    </head>
    <body>
    <h1>404 Not Found</h1>
    <p>The requested URL was not found on this server.</p>
    <p>URL: {url}</p>
    </body>
    </html>
    """));
    });
};
```

### Static Resource
```csharp
string staticResourcePath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "build");
string apiRegex = @"/api/v\d*/";
UrlRouter.Register([@$"^(?!{apiRegex})(?<filePath>.*)$"], async (Session session, string filePath) =>
{
    // Logger.Info($"static file request: {filePath}");
    await Task.CompletedTask;
    session.Response.Headers.CacheControl = new CacheControlHeaderValue()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(30)
    };
    if (filePath.Contains('.'))
    {
        string fullPath = Path.Combine(staticResourcePath, filePath.TrimStart('/'));
        return new DetectFile(fullPath, filePath);
    }
    else
    {
        string fullPath = Path.Combine(staticResourcePath, filePath.TrimStart('/'));
        while (true)
        {
            var indexFilePath = Path.Combine(fullPath, "index.html");
            if (File.Exists(indexFilePath))
            {
                return new DetectFile(indexFilePath, filePath);
            }
            if (fullPath == staticResourcePath)
            {
                break;
            }
            fullPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
        }
        return new DetectFile(Path.Combine(staticResourcePath, "index.html"), filePath);
    }
});
```

### Cross Origin
```csharp
urlRouter.Filter.Register(0, [".*"], async (Session session) =>
{
    await Task.CompletedTask;
    session.Response.Headers.SetHeader("Access-Control-Allow-Origin", "*");
    session.Response.Headers.SetHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    session.Response.Headers.SetHeader("Access-Control-Allow-Headers", "*");
    session.Response.Headers.SetHeader("Access-Control-Allow-Credentials", "true");

    if (session.Request.Method == UrlMethods.HTTP_OPTIONS)
    {
        return false;
    }
    return true;
});
```

### Get Request Body Stream
```csharp
urlRouter.Register(["/api/v1/hello"],async(Stream stream)=>{
    // You can read the request body stream
});
```

### Set Response Body Stream
```csharp
urlRouter.Register(["/api/v1/echo"],async(Stream stream)=>{
    MemoryStream memoryStream=new();
    stream.CopyTo(memoryStream);
    return memoryStream;
});
```

### Parse parameters from query string / BodyJson / Cache
```csharp
urlRouter.Register(["/api/v1/hi"], async (string name) => {
    // The field 'name' will be obtained from either the query string or the body JSON or Cache
    return $"hi~ {name}";
});
```