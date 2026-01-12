using TidyHPC.LiteJson;
using static TidyHPC.Loggers.LoggerFile;

namespace TidyHPC.Loggers;

/// <summary>
/// Global logger
/// </summary>
public class Logger
{
    /// <summary>
    /// 日志文件
    /// </summary>
    public static LoggerFile LoggerFile { get; private set; } = new();

    /// <summary>
    /// Set logger file
    /// </summary>
    /// <param name="loggerFile"></param>
    public static void SetLoggerFile(LoggerFile loggerFile)
    {
        LoggerFile.Dispose();
        LoggerFile = loggerFile;
    }

    /// <summary>
    /// The log level
    /// </summary>
    public static Levels Level
    {
        get => LoggerFile.Level;
        set => LoggerFile.Level = value;
    }

    /// <summary>
    /// The length of the parameter
    /// </summary>
    public static int ParameterKeyLength
    {
        get => LoggerFile.ParameterKeyLength;
        set => LoggerFile.ParameterKeyLength = value;
    }

    /// <summary>
    /// Machine name
    /// </summary>
    public static string MachineName
    {
        get => LoggerFile.MachineName;
        set => LoggerFile.MachineName = value;
    }

    /// <summary>
    /// Format the log message
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="excetion"></param>
    /// <returns></returns>
    private static string Format(Levels level, string message, Exception? excetion)
        => LoggerFile.Format(level, message, excetion);

    /// <summary>
    /// Format the log message
    /// </summary>
    /// <param name="level"></param>
    /// <param name="excetion"></param>
    /// <returns></returns>
    private static string Format(Levels level, Exception? excetion)
        => LoggerFile.Format(level, excetion);

    private static string FormatParameter(string key, string value)
        => LoggerFile.FormatParameter(key, value);

    private static string FormatLinear(string value)
        => LoggerFile.FormatLinear(value);

    /// <summary>
    /// 日志路径
    /// </summary>
    public static string FilePath
    {
        get => LoggerFile.FilePath;
        set => LoggerFile.FilePath = value;
    }

    /// <summary>
    /// 是否写入标准输出流
    /// </summary>
    public static bool EnableWriteToStandardOutpuStream
    {
        get => LoggerFile.EnableWriteToStandardOutpuStream;
        set => LoggerFile.EnableWriteToStandardOutpuStream = value;
    }

    /// <summary>
    /// Write line
    /// </summary>
    /// <param name="message"></param>
    public static void WriteLine(object? message) => LoggerFile.WriteLine(message);

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void Error(string message, Exception? exception = null) => LoggerFile.Error(message, exception);

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="exception"></param>
    public static void Error(Exception? exception) => LoggerFile.Error(exception);

    /// <summary>
    /// Error parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void ErrorParameter(string key, string value, Exception? exception = null) => LoggerFile.ErrorParameter(key, value, exception);

    /// <summary>
    /// Error parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void ErrorParameters(string message, Json parameters, Exception? exception = null)
        => LoggerFile.ErrorParameters(message, parameters, exception);

    /// <summary>
    /// Error linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void ErrorLinear(string message, Exception? exception = null)
        => LoggerFile.ErrorLinear(message, exception);

    /// <summary>
    /// Info
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void Info(string message, Exception? exception = null)
        => LoggerFile.Info(message, exception);

    /// <summary>
    /// Info linear
    /// </summary>
    public static void InfoLinear()
        => LoggerFile.InfoLinear();

    /// <summary>
    /// Info linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void InfoLinear(string message, Exception? exception = null)
        => LoggerFile.InfoLinear(message, exception);

    /// <summary>
    /// Info parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void InfoParameter(string key, string value, Exception? exception = null)
        => LoggerFile.InfoParameter(key, value, exception);

    /// <summary>
    /// Info parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void InfoParameters(string message, Json parameters, Exception? exception = null)
        => LoggerFile.InfoParameters(message, parameters, exception);

    /// <summary>
    /// Info parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void InfoParameters(Json parameters, Exception? exception = null)
        => LoggerFile.InfoParameters(parameters, exception);

    /// <summary>
    /// Debug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void Debug(string message, Exception? exception = null)
        => LoggerFile.Debug(message, exception);

    /// <summary>
    /// Debug parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void DebugParameter(string key, string value, Exception? exception = null)
        => LoggerFile.DebugParameter(key, value, exception);

    /// <summary>
    /// Debug parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void DebugParameters(string message, Json parameters, Exception? exception = null)
        => LoggerFile.DebugParameters(message, parameters, exception);

    /// <summary>
    /// Debug parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void DebugParameters(Json parameters, Exception? exception = null)
        => LoggerFile.DebugParameters(parameters, exception);

    /// <summary>
    /// Debug linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void DebugLinear(string message, Exception? exception = null)
        => LoggerFile.DebugLinear(message, exception);

    /// <summary>
    /// Develop
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void Develop(string message, Exception? exception = null)
        => LoggerFile.Develop(message, exception);

    /// <summary>
    /// Warn
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void Warn(string message, Exception? exception = null)
        => LoggerFile.Warn(message, exception);

    /// <summary>
    /// Warn linear
    /// </summary>
    public static void WarnLinear()
        => LoggerFile.WarnLinear();

    /// <summary>
    /// Warn linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public static void WarnLinear(string message, Exception? exception = null)
        => LoggerFile.WarnLinear(message, exception);

    /// <summary>
    /// Warn parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void WarnParameter(string key, string value, Exception? exception = null)
        => LoggerFile.WarnParameter(key, value, exception);

    /// <summary>
    /// Develop linear
    /// </summary>
    public static void DevelopLinear()
        => LoggerFile.DevelopLinear();

    /// <summary>
    /// Develop parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public static void DevelopParameter(string key, string value, Exception? exception = null)
        => LoggerFile.DevelopParameter(key, value, exception);

    /// <summary>
    /// Develop parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void DevelopParameters(string message, Json parameters, Exception? exception = null)
        => LoggerFile.DevelopParameters(message, parameters, exception);

    /// <summary>
    /// Develop parameters  
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public static void DevelopParameters(Json parameters, Exception? exception = null)
        => LoggerFile.DevelopParameters(parameters, exception);

}
