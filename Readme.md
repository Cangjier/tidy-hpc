
# TidyHPC
- LiteJson: Compared to System.Text.Json, it offers stronger compatibility; internal objects can be either System.Text.Json or generic List/Dictionary Objects.
- LiteDB: A lightweight database, similar to IndexDB.
- LiteHttpServer: A server encapsulated based on HttpListener, mainly used together with UrlRouter.
- UrlRouter: URL routing; you only need to provide a Session for routing, supporting both Filter and Router.

## UrlRouter
### Create
```csharp
UrlRouter urlRouter = new();
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
urlRouter.Register(["/api/v1/hello"], async (Stream stream) => {
    // You can read the request body stream here
});
```

### Set Response Body Stream
```csharp
urlRouter.Register(["/api/v1/echo"], async (Stream stream) => {
    MemoryStream memoryStream = new();
    stream.CopyTo(memoryStream);
    return memoryStream;
});
```

### Get Request Body As Json
```csharp
urlRouter.Register(["/api/v1/echo"], async (Session session) => {
    var bodyJson = await session.Cache.GetRequestBodyJson();
});
```

### Get Request Body As String
```csharp
urlRouter.Register(["/api/v1/echo"], async (Session session) => {
    var bodyString = await session.Cache.GetRequestBodyString();
});
```

### Parse parameters from query string / body json / cache
```csharp
urlRouter.Register(["/api/v1/hi"], async (string name) => {
    // The field 'name' will be obtained from either the query string, the body JSON, or the cache
    return $"hi~ {name}";
});
```

### Parse parameters by deserializing from body json

```csharp
// Suppose there is a class Person
urlRouter.Register(["/api/v1/hi"], async (Person person) => {
    // In this case, the request Body should be
    // {"person":{...}}, and the 'person' field will be automatically parsed and deserialized to the Person person parameter
});
```

### Set/Use Cache
```csharp
// Suppose there is an interface A
InterfaceA a = new ImplementationA();
urlRouter.Filter.Register(0, [".*"], async (Session session) =>
{
    // Set InterfaceA into the context
    session.Cache.Data.Set<InterfaceA>(a);
});

urlRouter.Register(["/api/v1/say"], async (InterfaceA a) =>
{
    // Retrieve InterfaceA from context
});
```
