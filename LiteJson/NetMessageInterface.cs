using TidyHPC.Loggers;

namespace TidyHPC.LiteJson;

/// <summary>
/// NetMessage接口
/// </summary>
/// <param name="target"></param>
public class NetMessageInterface(Json target):IDisposable
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicit convertion from Json to NetMessageInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator NetMessageInterface(Json target) => new (target);

    /// <summary>
    /// Implicit convertion from NetMessageInterface to Json
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator Json(NetMessageInterface target) => target.Target;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid id
    {
        get => Target.Read("id", Guid.Empty);
        set => Target.Set("id", value);
    }

    /// <summary>
    /// 消息代码
    /// </summary>
    public string code
    {
        get => Target.Read("code", string.Empty) ?? string.Empty;
        set => Target.Set("code", value);
    }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string message
    {
        get => Target.Read("message", string.Empty) ?? string.Empty;
        set
        {
            Target.Set("message", value);
        }
    }

    /// <summary>
    /// 消息是否成功
    /// </summary>
    public bool success
    {
        get => Target.Read("success", false);
        set => Target.Set("success", value);
    }

    /// <summary>
    /// 数据
    /// </summary>
    public Json data
    {
        get => Target.Get("data", Json.Null);
        set => Target.Set("data", value);
    }

    /// <summary>
    /// 跟踪信息
    /// </summary>
    public TraceInterface Trace
    {
        get => Target.GetOrCreateObject("Trace");
        set => Target.Set("Trace", value);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="popOuterFunctionCount"></param>
    public void Error(int? code, string? message, Exception? exception,int popOuterFunctionCount)
    {
        if (code != null)
        {
            this.code = code.Value.ToString();
        }
        if (success && message != null)
        {
            this.message = message;
        }
        success = false;
        Trace.Log(LoggerFile.Levels.Error, message, exception, popOuterFunctionCount);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Error(int? code, string? message, Exception? exception = null)
    {
        if (code != null)
        {
            this.code = code.Value.ToString();
        }
        if (success && message != null)
        {
            this.message = message;
        }
        success = false;
        Trace.Log(LoggerFile.Levels.Error, message, exception);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Error(string message, Exception? exception = null)
    {
        Error(null, message, exception);
    }

    /// <summary>
    /// 信息
    /// </summary>
    /// <param name="message"></param>
    public void Info(string message)
    {
        this.message = message;
        Trace.Log(LoggerFile.Levels.Info, message);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Target.Dispose();
    }

    /// <summary>
    /// 新建消息
    /// </summary>
    /// <returns></returns>
    public static NetMessageInterface New()
    {
        NetMessageInterface result = Json.NewObject();
        result.success = true;
        result.data = Json.Null;
        return result;
    }

    /// <summary>
    /// 转换成字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Target.ToString();
    }
}