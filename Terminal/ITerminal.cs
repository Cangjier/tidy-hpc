namespace TidyHPC.Terminal;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TidyHPC.Terminal.Linux;
using TidyHPC.Terminal.Windows;

/// <summary>
/// 终端接口
/// </summary>
public interface ITerminal : IDisposable
{
    /// <summary>
    /// 终端唯一标识符
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// 输出缓冲区大小
    /// </summary>
    public int OutputBufferSize { get; }

    /// <summary>
    /// 创建终端实例
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static ITerminal CreateTerminal(TerminalOptions options)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsTerminal(options);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxTerminal(options);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new NotImplementedException("macOS终端尚未实现");
        }
        else
        {
            throw new PlatformNotSupportedException("不支持的操作系统");
        }
    }

    /// <summary>
    /// 获取默认Shell路径
    /// </summary>
    /// <returns></returns>
    public static string GetDefaultShell()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "cmd.exe";
        }
        else
        {
            return "/bin/bash";
        }
    }

    /// <summary>
    /// 终端输出事件
    /// </summary>
    event Func<byte[], int, Task> OutputReceived;

    /// <summary>
    /// 启动终端
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 向终端输入数据
    /// </summary>
    Task WriteInputAsync(byte[] buffer, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 调整终端大小
    /// </summary>
    Task ResizeAsync(int columns, int rows, CancellationToken cancellationToken = default);
}

/// <summary>
/// 终端选项
/// </summary>
public class TerminalOptions
{
    /// <summary>
    /// 工作目录
    /// </summary>
    public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];

    /// <summary>
    /// 终端列数
    /// </summary>
    public int Columns { get; set; } = 80;

    /// <summary>
    /// 终端行数
    /// </summary>
    public int Rows { get; set; } = 24;

    /// <summary>
    /// 输出缓冲区大小
    /// </summary>
    public int OutputBufferSize { get; set; } = 1024;

    /// <summary>
    /// 使用的Shell
    /// </summary>
    public string Shell { get; set; } = ITerminal.GetDefaultShell();
}