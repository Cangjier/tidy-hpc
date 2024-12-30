using TidyHPC.LiteJson;

namespace TidyHPC.Loggers;

/// <summary>
/// 日志文件
/// </summary>
public class LoggerFile:IDisposable
{
    /// <summary>
    /// Log level
    /// </summary>
    public enum Levels
    {
        /// <summary>
        /// Log all
        /// </summary>
        Debug,
        /// <summary>
        /// Log info and above
        /// </summary>
        Info,
        /// <summary>
        /// Log error and above
        /// </summary>
        Error,
        /// <summary>
        /// Log fatal only
        /// </summary>
        None
    }

    /// <summary>
    /// The log level
    /// </summary>
    public Levels Level { get; set; } = Levels.Debug;

    /// <summary>
    /// The length of the parameter
    /// </summary>
    public int ParameterKeyLength { get; set; } = 32;

    /// <summary>
    /// The log queue
    /// </summary>
    public QueueLogger QueueLogger { get; } = new QueueLogger();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        QueueLogger.Dispose();
    }

    /// <summary>
    /// Machine name
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// Format the log message
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="excetion"></param>
    /// <returns></returns>
    public string Format(Levels level, string message, Exception? excetion)
    {
        return string.Format("[{0}] {1} [{2}] {3} {4}", MachineName, DateTime.Now.ToString("O"), level, message, excetion);
    }

    /// <summary>
    /// Format the log message
    /// </summary>
    /// <param name="level"></param>
    /// <param name="excetion"></param>
    /// <returns></returns>
    public string Format(Levels level, Exception? excetion)
    {
        return string.Format("[{0}] {1} [{2}] {3}", MachineName, DateTime.Now.ToString("O"), level, excetion);
    }

    /// <summary>
    /// Format parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public string FormatParameter(string key, string value)
    {
        return key + ' '.Repeat(ParameterKeyLength - key.Length) + "=" + value;
    }

    /// <summary>
    /// Format linear
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string FormatLinear(string value)
    {
        int next = ParameterKeyLength * 2 - value.Length;
        int half = next / 2;
        return '-'.Repeat(next - half) + value + '-'.Repeat(half);
    }

    /// <summary>
    /// 日志路径
    /// </summary>
    public string FilePath
    {
        get => QueueLogger.FilePath;
        set => QueueLogger.FilePath = value;
    }

    /// <summary>
    /// Write line
    /// </summary>
    /// <param name="message"></param>
    public void WriteLine(object? message)
    {
        QueueLogger.WriteLine(message?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Error(string message, Exception? exception = null)
    {
        QueueLogger.WriteLine(Format(Levels.Error, message, exception));
    }

    /// <summary>
    /// Error
    /// </summary>
    /// <param name="exception"></param>
    public void Error(Exception? exception)
    {
        QueueLogger.WriteLine(Format(Levels.Error, exception));
    }

    /// <summary>
    /// Error parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public void ErrorParameter(string key, string value, Exception? exception = null)
    {
        Error(FormatParameter(key, value), exception);
    }

    /// <summary>
    /// Error parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public void ErrorParameters(string message, Json parameters, Exception? exception = null)
    {
        Error(message, exception);
        QueueLogger.WriteLine(parameters.ToString(true));
    }

    /// <summary>
    /// Error linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void ErrorLinear(string message, Exception? exception = null)
    {
        Error(FormatLinear(message), exception);
    }

    /// <summary>
    /// Info
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Info(string message, Exception? exception = null)
    {
        QueueLogger.WriteLine(Format(Levels.Info, message, exception));
    }

    /// <summary>
    /// Info linear
    /// </summary>
    public void InfoLinear()
    {
        Info(FormatLinear(string.Empty), null);
    }

    /// <summary>
    /// Info linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void InfoLinear(string message, Exception? exception = null)
    {
        Info(FormatLinear(message), exception);
    }

    /// <summary>
    /// Info parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public void InfoParameter(string key, string value, Exception? exception = null)
    {
        Info(FormatParameter(key, value), exception);
    }

    /// <summary>
    /// Info parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public void InfoParameters(string message, Json parameters, Exception? exception = null)
    {
        Info(message, exception);
        QueueLogger.WriteLine(parameters.ToString(true));
    }

    /// <summary>
    /// Info parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public void InfoParameters(Json parameters, Exception? exception = null)
    {
        Info(string.Empty, exception);
        QueueLogger.WriteLine(parameters.ToString(true));
    }

    /// <summary>
    /// Debug
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Debug(string message, Exception? exception = null)
    {
        QueueLogger.WriteLine(Format(Levels.Debug, message, exception));
    }

    /// <summary>
    /// Debug parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="exception"></param>
    public void DebugParameter(string key, string value, Exception? exception = null)
    {
        Debug(FormatParameter(key, value), exception);
    }

    /// <summary>
    /// Debug parameters
    /// </summary>
    /// <param name="message"></param>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public void DebugParameters(string message, Json parameters, Exception? exception = null)
    {
        Debug(message, exception);
        QueueLogger.WriteLine(parameters.ToString(true));
    }

    /// <summary>
    /// Debug parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="exception"></param>
    public void DebugParameters(Json parameters, Exception? exception = null)
    {
        Debug(string.Empty, exception);
        QueueLogger.WriteLine(parameters.ToString(true));
    }

    /// <summary>
    /// Debug linear
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void DebugLinear(string message, Exception? exception = null)
    {
        Debug(FormatLinear(message), exception);
    }
}
