namespace TidyHPC.Terminal;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TidyHPC.Terminal.Linux;
using TidyHPC.Terminal.Windows;

public interface ITerminal : IDisposable
{
    public static ITerminal CreateTerminal()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsTerminal();
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxTerminal();
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
    event Action<byte[],int> OutputReceived;

    /// <summary>
    /// 启动终端
    /// </summary>
    /// <param name="shell">Shell路径（如：/bin/bash, cmd.exe）</param>
    /// <param name="columns">终端列数</param>
    /// <param name="rows">终端行数</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task<bool> StartAsync(TerminalOptions options,CancellationToken cancellationToken = default);

    /// <summary>
    /// 向终端输入数据
    /// </summary>
    Task WriteInputAsync(byte[] buffer, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 调整终端大小
    /// </summary>
    Task ResizeAsync(int columns, int rows, CancellationToken cancellationToken = default);
}

public class TerminalOptions
{
    public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;
    public Dictionary<string, string> EnvironmentVariables { get; set; } = [];
    public int Columns { get; set; } = 80;
    public int Rows { get; set; } = 24;
    public string Shell { get; set; } = ITerminal.GetDefaultShell();
}