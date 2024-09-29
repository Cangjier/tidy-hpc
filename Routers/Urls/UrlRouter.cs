﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Urls.Exceptions;
using TidyHPC.Routers.Urls.Interfaces;
using TidyHPC.Routers.Urls.Responses;
using TidyHPC.Routers.Urls.Utils;

namespace TidyHPC.Routers.Urls;

/// <summary>
/// 路由记录
/// </summary>
/// <param name="Pattern"></param>
/// <param name="Regex"></param>
/// <param name="Handler"></param>
public record UrlRouterRecord(string Pattern, Regex Regex, Func<Session, Task> Handler);

/// <summary>
/// Url 方法特性
/// </summary>
/// <param name="Parameter"></param>
/// <param name="Aliases"></param>
/// <param name="IsOptional"></param>
public record UrlParameterMetaRecord(ParameterInfo Parameter, string[] Aliases,bool IsOptional);

/// <summary>
/// Url 路由事件
/// </summary>
/// <param name="urlRouter"></param>
public class UrlRouterEvents(UrlRouter urlRouter)
{
    /// <summary>
    /// Url 路由
    /// </summary>
    public UrlRouter UrlRouter { get; } = urlRouter;

    /// <summary>
    /// 在路由之前触发
    /// </summary>
    public Func<string, Session, Task<bool>>? OnBeforeRouteAsync { get; set; }

    internal async Task<bool> BeforeRouteAsync(string url, Session session)
    {
        if (OnBeforeRouteAsync != null)
        {
            return await OnBeforeRouteAsync(url, session);
        }
        return true;
    }

    /// <summary>
    /// 在路由之后，处理之前触发
    /// </summary>
    public Func<string, Session, Task<bool>>? OnBeforeHandlerAsync { get; set; }

    internal async Task<bool> BeforeHandlerAsync(string url, Session session)
    {
        if (OnBeforeHandlerAsync != null)
        {
            return await OnBeforeHandlerAsync(url, session);
        }
        return true;
    }

    /// <summary>
    /// 在处理之后触发
    /// </summary>
    public Func<string, Session,Task>? OnAfterHandler { get; set; }

    internal async Task AfterHandler(string url, Session session)
    {
        if (OnAfterHandler != null)
        {
            await OnAfterHandler(url, session);
        }
    }

    /// <summary>
    /// 当没有路由时触发
    /// </summary>
    public Func<string,Session,Task>? OnNoRoute { get; set; }

    internal async Task NoRoute(string url, Session session)
    {
        if (OnNoRoute != null)
        {
            await OnNoRoute(url, session);
        }
        else
        {
            throw new NoRouterException(url);
        }
    }

    /// <summary>
    /// 在响应Json生成之前触发
    /// </summary>
    public Func<Session,Json,Task>? OnResponseJsonGenerated { get; set; }

    internal async Task ResponseJsonGenerated(Session session,Json responseJson)
    {
        if (OnResponseJsonGenerated != null)
        {
            await OnResponseJsonGenerated(session, responseJson);
        }
    }
}

/// <summary>
/// 路由器
/// </summary>
public class UrlRouter
{
    /// <summary>
    /// Url 路由器
    /// </summary>
    public UrlRouter()
    {
        Events = new UrlRouterEvents(this);
        Filter = new UrlFilter(this);
    }

    /// <summary>
    /// 实际的路由映射
    /// </summary>
    private ConcurrentDictionary<string, UrlRouterRecord> RealMap { get; } = new();

    /// <summary>
    /// 热更新的路由映射
    /// </summary>
    private ConcurrentDictionary<string, UrlRouterRecord> HotMap { get; } = new();

    /// <summary>
    /// 热更新的无命中路由Url
    /// </summary>
    private HashSet<string> HotNoRouterUrls { get; } = new();

    /// <summary>
    /// 热更新的Url正则组，key是Url
    /// </summary>
    internal ConcurrentDictionary<string, ImmutableDictionary<string,string>> HotUrlRegexMatchGroups { get; } = new();

    /// <summary>
    /// 过滤器
    /// </summary>
    public UrlFilter Filter { get; }

    /// <summary>
    /// 路由事件
    /// </summary>
    public UrlRouterEvents Events { get; }

    /// <summary>
    /// 路由
    /// </summary>
    /// <param name="url"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public async Task Route(string url,Session session)
    {
        if (await Filter.Filter(url, session) == UrlFilterStatus.Rejected) return;
        if (await Events.BeforeRouteAsync(url, session) == false) return;
        if (HotMap.ContainsKey(url))
        {
            if (HotUrlRegexMatchGroups.TryGetValue(url, out var urlRegexMatchGroups))
            {
                session.Cache.SetUrlRegexMatchGroups(urlRegexMatchGroups);
            }
            if (await Events.BeforeHandlerAsync(url, session) == false) return;
            await HotMap[url].Handler(session);
            await Events.AfterHandler(url, session);
        }
        else
        {
            if (HotNoRouterUrls.Contains(url))
            {
                await Events.NoRoute(url, session);
                return;
            }
            foreach (var item in RealMap)
            {
                var matchResult = item.Value.Regex.Match(url);
                if (matchResult.Success)
                {
                    session.Cache.SetUrlRegexMatchGroups(matchResult.Groups);
                    HotUrlRegexMatchGroups.TryAdd(url, session.Cache.UrlRegexMatchGroups!);
                    HotMap.TryAdd(url, item.Value);
                    if (await Events.BeforeHandlerAsync(url, session) == false) return;
                    try
                    {
                        await item.Value.Handler(session);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                    await Events.AfterHandler(url, session);
                    return;
                }
            }
            HotNoRouterUrls.Add(url);
            await Events.NoRoute(url, session);
        }
    }

    /// <summary>
    /// 监听服务
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    public async Task Listen(IServer server)
    {
        while (true)
        {
            var session = await server.GetNextSession();
            string? url = null;
            try
            {
                if (session.Request.Url != null)
                {
                    if (session.Request.Url.IsAbsoluteUri) url = session.Request.Url.AbsolutePath;
                    else url = session.Request.Url.OriginalString;
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                continue;
            }
            if (url != null)
            {
                _ = Task.Run(async () =>
                {
                    try 
                    {
                        await Route(url, session);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e);
                    }
                    finally
                    {
                        session.Dispose();
                    }
                });
            }
            else
            {
                Logger.Error("Url is null");
            }
        }
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="method"></param>
    /// <param name="onInstance"></param>
    /// <param name="onPattern"></param>
    public void Register(MethodInfo method, Func<string> onPattern, Func<object?> onInstance)
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
            parameterMetas[i] = new UrlParameterMetaRecord(parameter, aliases, isOptional);
        }

        Type? taskResultType = null;
        PropertyInfo? taskResultProperty = null;
        bool methodReturnTypeIsTask = false;
        if (method.ReturnType.BaseType == typeof(Task))
        {
            methodReturnTypeIsTask = true;
            var genericTypeArguments = method.ReturnType.GenericTypeArguments;
            if (genericTypeArguments.Length != 1)
            {
                throw new NotImplementedException();
            }
            taskResultType = genericTypeArguments[0];
            taskResultProperty = method.ReturnType.GetProperty("Result");
        }
        var sendError =async (Session session,Action<NetMessageInterface> onMessage) =>
        {
            await session.Complete(async () =>
            {
                session.Response.Headers.ContentEncoding = UrlResponse.DefaultContentEncoding;
                session.Response.Headers.ContentType = new Headers.ContentType()
                {
                    MediaType = "application/json"
                };
                using NetMessageInterface resultJson = NetMessageInterface.New();
                await Events.ResponseJsonGenerated(session, resultJson.Target);
                onMessage(resultJson);

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
                else if(UrlResponse.DefaultContentEncoding=="deflate")
                {
                    using DeflateStream deflateStream = new(session.Response.Body, CompressionMode.Compress);
                    resultJson.Target.WriteTo(deflateStream);
                }
                else
                {
                    resultJson.Target.WriteTo(session.Response.Body);
                }
            });
        };

        RealMap.TryAdd(urlPattern, new UrlRouterRecord(urlPattern, urlRegex, async session =>
        {
            var queryStrings = session.Request.Query;
            Json bodyJson = Json.Null;
            
            try
            {
                // 解析请求体，可能存在异常
                bodyJson = await session.Cache.GetRequstBodyJson();
            }
            catch(Exception e)
            {
                Logger.Error(e);
                await sendError(session, msg => msg.Error(null, "解析请求体时发生异常", e));
                return;
            }
            if (urlMethod != null && urlMethod.Method != session.Request.Method)
            {
                await sendError(session, msg => msg.Error(null, $"请求方法不匹配,预期方法为{urlMethod.Method},实际方法为{session.Request.Method}"));
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
                    else if (bodyJson.TryGet(aliases, parameter.ParameterType, out var bodyDataValue, () => throw new Exception($"参数{string.Join(',', aliases)}无法转换为{parameter.ParameterType}")))
                    {
                        arguments[i] = bodyDataValue;
                    }
                    else if(session.Cache.TryGetUrlRegexMatchGroup(aliases,out string? urlRegexGroupValue))
                    {
                        if(urlRegexGroupValue.TryConvertTo(parameter.ParameterType,out var urlRegexGroupValueConvert))
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
            catch(Exception e)
            {
                Logger.Error(e);
                await sendError(session, msg => msg.Error(-1, e.Message, e));
                return;
            }

            object? result = null;
            try
            {
                result = method.Invoke(instance, arguments);
                if (result is Task resultTask) await resultTask;
            }
            catch(Exception e)
            {
                await sendError(session, msg => msg.Error(-1, e.Message, e));
                return;
            }
            await session.Complete(async () =>
            {
                if (methodReturnTypeIsTask)
                {
                    var resultValue = taskResultProperty?.GetValue(result);
                    await session.Setter.SetResponse(resultValue, this);
                }
                else
                {
                    await session.Setter.SetResponse(result, this);
                }
            });
        }));
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="urlAliases"></param>
    /// <param name="onInstance"></param>
    /// <param name="method"></param>
    public void Register(string[] urlAliases, MethodInfo method, Func<object?> onInstance)
    {
        Register(method, () =>
        {
            var urlPattern = $"^({string.Join('|', urlAliases)})$";
            return urlPattern;
        }, onInstance);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="urlAliases"></param>
    /// <param name="method"></param>
    public void Register(string[] urlAliases, Delegate method)
    {
        Register(urlAliases, method.Method, () => method.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register(string[] urlAliases, Func<Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T>(string[] urlAliases, Func<T, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2>(string[] urlAliases, Func<T1, T2, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3>(string[] urlAliases, Func<T1, T2, T3, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4>(string[] urlAliases, Func<T1, T2, T3, T4, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5>(string[] urlAliases, Func<T1, T2, T3, T4, T5, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, T7, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<T1, T2, T3, T4, T5, T6, T7, T8>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult>(string[] urlAliases, Func<Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult,T1>(string[] urlAliases, Func<T1,Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2>(string[] urlAliases, Func<T1, T2, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3>(string[] urlAliases, Func<T1, T2, T3, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3, T4>(string[] urlAliases, Func<T1, T2, T3, T4, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3, T4, T5>(string[] urlAliases, Func<T1, T2, T3, T4, T5, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3, T4, T5, T6>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3, T4, T5, T6, T7>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }

    /// <summary>
    /// 通过方法反射注册路由
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="T8"></typeparam>
    /// <param name="urlAliases"></param>
    /// <param name="func"></param>
    public void Register<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(string[] urlAliases, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> func)
    {
        Register(urlAliases, func.Method, () => func.Target);
    }
}
