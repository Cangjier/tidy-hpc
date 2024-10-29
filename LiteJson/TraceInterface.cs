using System.Diagnostics;
using System.Reflection;
using System.Text;
using TidyHPC.Loggers;

namespace TidyHPC.LiteJson;

/// <summary>
/// 跟踪信息接口
/// </summary>
/// <param name="target"></param>
public struct TraceInterface(Json target)
{
    /// <summary>
    /// 封装对象
    /// </summary>
    public Json Target = target;

    /// <summary>
    /// Implicit conversion from TraceInterface to Json
    /// </summary>
    /// <param name="trace"></param>
    public static implicit operator Json(TraceInterface trace) => trace.Target;

    /// <summary>
    /// Implicit conversion from Json to TraceInterface
    /// </summary>
    /// <param name="target"></param>
    public static implicit operator TraceInterface(Json target) => new(target);

    /// <summary>
    /// 跟踪期间发生错误，则值为false
    /// </summary>
    public bool Success
    {
        get => Target.Read("Success", true);
        set => Target.Set("Success", value);
    }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message
    {
        get => Target.Read("Message", string.Empty) ?? string.Empty;
        set => Target.Set("Message", value);
    }

    /// <summary>
    /// 错误日志
    /// </summary>
    public Json ErrorLogger => Target.GetOrCreateArray("ErrorLogger");

    /// <summary>
    /// 信息日志
    /// </summary>
    public Json InfoLogger => Target.GetOrCreateArray("InfoLogger");

    /// <summary>
    /// 调试日志
    /// </summary>
    public Json DebugLogger => Target.GetOrCreateArray("DebugLogger");

    /// <summary>
    /// 添加跟踪日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Error(string? message,Exception? exception=null)
    {
        if (message != null)
        {
            if (Message == "")
            {
                Message = message;
            }
        }
        else if(exception != null)
        {
            if (Message == "")
            {
                Message = exception.Message;
            }
        }
        Success = false;
        Log(Logger.Levels.Error, message, exception);
    }

    /// <summary>
    /// 添加跟踪日志
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Info(string? message, Exception? exception=null)
    {
        Log(Logger.Levels.Info, message, exception);
    }

    /// <summary>
    /// 更新跟踪信息
    /// </summary>
    /// <param name="trace"></param>
    /// <param name="overwrite">强行使用trace中的</param>
    public void Update(TraceInterface trace,bool overwrite = false)
    {
        if (overwrite)
        {
            Success = trace.Success;
            Message = trace.Message;
        }
        else
        {
            if (trace.Success == false)
            {
                Success = false;
            }
            if (!string.IsNullOrEmpty(trace.Message))
            {
                Message = trace.Message;
            }
        }
        foreach (var (key,value) in trace.Target.GetObjectEnumerable())
        {
            if (Target.ContainsKey(key))
            {
                if (value.IsArray)
                {
                    Target.Get(key).AddRange(value);
                }
            }
            else
            {
                Target[key] = value.Clone();
            }
        }
    }

    /// <summary>
    /// Logger the message with level and exception and trace
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="popOuterFunctionCount"></param>
    /// <param name="showTrace"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Log(Logger.Levels level, string? message, Exception? exception = null, int popOuterFunctionCount = 0, bool showTrace = true)
    {
        Json logger = level switch
        {
            Logger.Levels.Error => ErrorLogger,
            Logger.Levels.Info => InfoLogger,
            Logger.Levels.Debug => DebugLogger,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
        if (message != null)
        {
            StringBuilder loggerLine = new();
            loggerLine.Append($"{DateTime.Now:O} {message}");
            if (showTrace)
            {
                var trace = new StackTrace(new StackFrame(2 + popOuterFunctionCount, true));
                if (trace.GetFrame(0) is StackFrame frame && frame.GetMethod() is MethodInfo frameMethod)
                {
                    var fileName = Path.GetFileName(frame.GetFileName());
                    var lineNumber = frame.GetFileLineNumber();
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        loggerLine.Append($", {fileName} - Line {lineNumber}");
                    }
                }
            }
            logger.Add(loggerLine.ToString());
        }
        if (exception != null)
        {
            foreach (var line in exception.ToString().Replace("\r", "").Split('\n'))
            {
                logger.Add(line);
            }
        }
    }
}