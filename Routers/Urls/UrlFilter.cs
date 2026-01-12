using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls.Responses;
using TidyHPC.Routers.Urls.Utils;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// 路由过滤记录
/// </summary>
/// <param name="Pattern"></param>
/// <param name="Regex"></param>
/// <param name="Handler"></param>
public record UrlRouterFilterRecord(string Pattern, Regex Regex, Func<Session, Task> Handler);

/// <summary>
/// 路由过滤器层记录，允许匹配重复
/// </summary>
/// <param name="Filters"></param>
public record UrlRouterFilterLayerReocrd(ConcurrentBag<UrlRouterFilterRecord> Filters);

/// <summary>
/// Url 过滤器
/// </summary>
/// <param name="urlRouter"></param>
public class UrlFilter(UrlRouter urlRouter)
{
    /// <summary>
    /// Url 路由
    /// </summary>
    public UrlRouter UrlRouter { get; } = urlRouter;

    /// <summary>
    /// 实际拦截器映射，按优先级排序，优先级越高越先执行，同一层无序
    /// </summary>
    internal ConcurrentDictionary<int, UrlRouterFilterLayerReocrd> RealFilterMap { get; } = new();

    /// <summary>
    /// 热更新的拦截器映射
    /// </summary>
    internal ConcurrentDictionary<string, ConcurrentDictionary<int, UrlRouterFilterLayerReocrd>> HotFilterMap { get; } = new();

    /// <summary>
    /// 热更新的Url正则组，key是Url:Pattern
    /// </summary>
    internal ConcurrentDictionary<string, ImmutableDictionary<string, string>> HotUrlRegexMatchGroups { get; } = new();

    /// <summary>
    /// 优先级
    /// </summary>
    internal int[] Orders { get; set; } = [];

    /// <summary>
    /// 热更新的无命中拦截器Url
    /// </summary>
    internal HashSet<string> HotNoFilterUrls { get; } = new();

    internal async Task<UrlFilterStatus> Filter(string url, Session session)
    {
        var tempOrders = Orders;
        if (HotFilterMap.TryGetValue(url, out var hotMap))
        {
            foreach (var order in tempOrders)
            {
                if (hotMap.TryGetValue(order, out var layer))
                {
                    foreach (var filter in layer.Filters)
                    {
                        if (HotUrlRegexMatchGroups.TryGetValue($"{url}:{filter.Pattern}", out var urlRegexMatchGroups))
                        {
                            session.Cache.SetUrlRegexMatchGroups(urlRegexMatchGroups);
                        }
                        await filter.Handler(session);
                        if (session.Cache.FilterStatus == UrlFilterStatus.Rejected)
                        {
                            return UrlFilterStatus.Rejected;
                        }
                    }
                }
            }
            return session.Cache.FilterStatus;
        }
        else
        {
            if (HotNoFilterUrls.Contains(url))
            {
                return UrlFilterStatus.Released;
            }
            hotMap = new();
            HotFilterMap.TryAdd(url, hotMap);
            bool matched = false;
            foreach (var order in tempOrders)
            {
                var layer = RealFilterMap[order];
                foreach (var filter in layer.Filters)
                {
                    var matchResult = filter.Regex.Match(url);
                    if (matchResult.Success)
                    {
                        matched = true;
                        session.Cache.SetUrlRegexMatchGroups(matchResult.Groups);
                        HotUrlRegexMatchGroups.TryAdd($"{url}:{filter.Pattern}", session.Cache.UrlRegexMatchGroups!);
                        if (hotMap.TryGetValue(order, out var hotLayer) == false)
                        {
                            hotLayer = new(new());
                            hotMap.TryAdd(order, hotLayer);
                        }
                        hotLayer.Filters.Add(filter);
                        if (session.Cache.FilterStatus == UrlFilterStatus.Released)
                        {
                            await filter.Handler(session);
                        }
                    }
                }
            }
            if (matched == false)
            {
                HotNoFilterUrls.Add(url);
            }
        }
        return session.Cache.FilterStatus;
    }

    /// <summary>
    /// 注册拦截器
    /// </summary>
    /// <param name="order">拦截器优先级</param>
    /// <param name="method"></param>
    /// <param name="onPattern"></param>
    /// <param name="onInstance"></param>
    public void Register(int order, MethodInfo method, Func<string> onPattern, Func<object?> onInstance)
    {
        var urlPattern = onPattern();
        var urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);
        var urlMethod = method.GetCustomAttribute<UrlMethodAttribute>();

        var parameters = method.GetParameters();
        var parameterMetas = new UrlParameterMetaRecord[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var aliases = parameter.GetCustomAttribute<ArgsAliasesAttribute>()?.Aliases;
            var isOptional = parameter.GetCustomAttribute<OptionalAttribute>() != null;
            if (aliases == null && parameter.Name != null)
            {
                aliases = [parameter.Name];
            }
            if (aliases == null)
            {
                throw new Exception($"参数{parameter.Name}未指定别名");
            }
            parameterMetas[i] = new UrlParameterMetaRecord(parameter, aliases, isOptional, null);
        }

        Type? taskResultType = null;
        PropertyInfo? taskResultProperty = null;
        if (method.ReturnType.BaseType == typeof(Task))
        {
            var genericTypeArguments = method.ReturnType.GenericTypeArguments;
            if (genericTypeArguments.Length != 1)
            {
                throw new NotImplementedException();
            }
            taskResultType = genericTypeArguments[0];
            taskResultProperty = method.ReturnType.GetProperty("Result");
        }
        var sendError = async (Session session, Action<NetMessageInterface> onMessage) =>
        {
            session.Cache.FilterStatus = UrlFilterStatus.Rejected;
            await session.Complete(async () =>
            {
                session.Response.Headers.ContentEncoding = UrlResponse.DefaultContentEncoding;
                session.Response.Headers.ContentType = new Headers.ContentType()
                {
                    MediaType = "application/json"
                };
                using NetMessageInterface resultJson = NetMessageInterface.New();
                onMessage(resultJson);
                if (session.IsWebSocket)
                {
                    if (session.WebSocketResponse != null)
                    {
                        await session.WebSocketResponse.SendMessage(resultJson.Target.ToString());
                    }
                }
                else
                {

                    if (UrlResponse.DefaultContentEncoding == "br")
                    {
                        using BrotliStream brotliStream = new(session.Response.Body, CompressionMode.Compress);
                        resultJson.Target.WriteTo(brotliStream);
                    }
                    else if (UrlResponse.DefaultContentEncoding == "gzip")
                    {
                        using GZipStream gzipStream = new(session.Response.Body, CompressionMode.Compress);
                        resultJson.Target.WriteTo(gzipStream);
                    }
                    else if (UrlResponse.DefaultContentEncoding == "deflate")
                    {
                        using DeflateStream deflateStream = new(session.Response.Body, CompressionMode.Compress);
                        resultJson.Target.WriteTo(deflateStream);
                    }
                    else
                    {
                        resultJson.Target.WriteTo(session.Response.Body);
                    }
                }
            });

        };
        if (RealFilterMap.TryGetValue(order, out var record) == false)
        {
            record = new UrlRouterFilterLayerReocrd(new());
            RealFilterMap.TryAdd(order, record);
            Orders = [.. RealFilterMap.Keys.Order()];
        }
        var handler = async (Session session) =>
        {
            var queryStrings = session.Request.Query;
            Json bodyJson = Json.Null;

            try
            {
                // 解析请求体，可能存在异常
                bodyJson = await session.Cache.GetRequstBodyJson();
            }
            catch (Exception e)
            {
                await sendError(session, msg => msg.Error(-1, "解析请求体时发生异常", e));
                return;
            }
            if (urlMethod != null && urlMethod.Method != session.Request.Method)
            {
                await sendError(session, msg => msg.Error(-1, $"请求方法不匹配,预期方法为{urlMethod.Method},实际方法为{session.Request.Method}"));
                return;
            }
            var instance = onInstance();
            var arguments = new object?[parameters.Length];
            try
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.ParameterType == typeof(Session))
                    {
                        arguments[i] = session;
                        continue;
                    }
                    else if (parameter.ParameterType == typeof(Stream))
                    {
                        arguments[i] = session.Request.Body;
                        continue;
                    }
                    var aliases = parameterMetas[i].Aliases;
                    var isOptional = parameterMetas[i].IsOptional;
                    if (queryStrings.TryGet(aliases, out string queryValue))
                    {
                        if (queryValue.TryConvertTo(parameter.ParameterType, out var parameterValue))
                        {
                            arguments[i] = parameterValue;
                        }
                        else
                        {
                            throw new Exception($"参数{string.Join(',', aliases)}无法转换为{parameter.ParameterType}");
                        }
                    }
                    else if (bodyJson.Get("data", Json.Null).TryGet(aliases, parameter.ParameterType, out var bodyDataValue, () => throw new Exception($"参数{string.Join(',', aliases)}无法转换为{parameter.ParameterType}")))
                    {
                        arguments[i] = bodyDataValue;
                    }
                    else if (session.Cache.TryGetUrlRegexMatchGroup(aliases, out string? urlRegexGroupValue))
                    {
                        if (urlRegexGroupValue.TryConvertTo(parameter.ParameterType, out var urlRegexGroupValueConvert))
                        {
                            arguments[i] = urlRegexGroupValueConvert;
                        }
                        else
                        {
                            throw new Exception($"参数{string.Join(',', aliases)}无法转换为{parameter.ParameterType}");
                        }
                    }
                    else
                    {
                        if (isOptional == false) throw new Exception($"参数`{string.Join(',', aliases)}`未找到");
                    }
                }
            }
            catch (Exception e)
            {
                await sendError(session, msg => msg.Error(-1, e.Message, e));
                return;
            }

            object? result = null;
            try
            {
                result = method.Invoke(instance, arguments);
                if (result is Task resultTask) await resultTask;
            }
            catch (TargetInvocationException targetInvocationException)
            {
                await sendError(session, mes => mes.Error(-1, targetInvocationException.InnerException?.Message ?? "", targetInvocationException.InnerException));
                return;
            }
            catch (AggregateException aggregateException)
            {
                await sendError(session, mes => mes.Error(-1,
                    aggregateException.InnerExceptions?.FirstOrDefault()?.Message ?? "",
                    aggregateException.InnerExceptions?.FirstOrDefault()));
                return;
            }
            catch (Exception e)
            {
                await sendError(session, msg => msg.Error(-1, e.Message, e));
                return;
            }
            if (taskResultProperty != null)
            {
                var resultValue = taskResultProperty.GetValue(result);
                if (resultValue is FilterResult filterResult)
                {
                    session.Cache.FilterStatus = filterResult.Status;
                }
                else
                {
                    Logger.Error($"返回值类型不是FilterResult");
                    throw new Exception($"返回值类型不是FilterResult");
                }
            }
        };
        record.Filters.Add(new(urlPattern, urlRegex, handler));
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="onInstance"></param>
    /// <param name="method"></param>
    public void Register(int order, string[] urlPatterns, MethodInfo method, Func<object?> onInstance)
    {
        Register(order, method, () =>
        {
            var urlPattern = $"{string.Join('|', urlPatterns)}";
            return urlPattern;
        }, onInstance);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register(int order, string[] urlPatterns, Func<Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T>(int order, string[] urlPatterns, Func<T, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T1, T2>(int order, string[] urlPatterns, Func<T1, T2, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3>(int order, string[] urlPatterns, Func<T1, T2, T3, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4>(int order, string[] urlPatterns, Func<T1, T2, T3, T4, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5>(int order, string[] urlPatterns, Func<T1, T2, T3, T4, T5, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册过滤器
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="order"></param>
    /// <param name="urlPatterns"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6>(int order, string[] urlPatterns, Func<T1, T2, T3, T4, T5, T6, Task<FilterResult>> func)
    {
        Register(order, urlPatterns, func.Method, () => func.Target);
    }
}
