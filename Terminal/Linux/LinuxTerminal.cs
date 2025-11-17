namespace TidyHPC.Terminal.Linux;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static TidyHPC.Terminal.Linux.Native.PtyApi;

/// <summary>
/// Linux 终端实现
/// </summary>
public class LinuxTerminal : ITerminal
{
    private const int BufferSize = 4096;
    private const int STDIN_FILENO = 0;
    private const int STDOUT_FILENO = 1;
    private const int STDERR_FILENO = 2;

    private int MasterFileDescription = -1;
    private int SlaveFileDescription = -1;
    private int ChildProcessID = -1;
    private bool IsDisposed = false;
    private Stream? MasterStream;
    private CancellationTokenSource? ReadCancellationSource;
    private Task? ReadTask;

    /// <summary>
    /// 终端唯一标识
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 终端输出事件
    /// </summary>
    public event Action<byte[], int>? OutputReceived;

    /// <summary>
    /// 启动终端
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<bool> StartAsync(TerminalOptions options, CancellationToken cancellationToken = default)
    {
        if (MasterFileDescription != -1)
            throw new InvalidOperationException("终端已经启动");

        try
        {
            // 创建 PTY
            if (openpty(out MasterFileDescription, out SlaveFileDescription, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == -1)
            {
                throw new InvalidOperationException($"创建 PTY 失败: {Marshal.GetLastWin32Error()}");
            }

            // 创建子进程
            ChildProcessID = fork();

            if (ChildProcessID == -1)
            {
                throw new InvalidOperationException($"创建子进程失败: {Marshal.GetLastWin32Error()}");
            }

            if (ChildProcessID == 0) // 子进程
            {
                ChildProcess(options);
                return false; // 子进程不会执行到这里
            }
            else // 父进程
            {
                close(SlaveFileDescription); // 父进程不需要 slave 端
                SlaveFileDescription = -1;

                // 创建流用于读写
                MasterStream = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(
                    (IntPtr)MasterFileDescription, false), FileAccess.ReadWrite, BufferSize);

                // 调整终端大小
                await ResizeAsync(options.Columns, options.Rows, cancellationToken);

                // 开始读取输出
                ReadCancellationSource = new CancellationTokenSource();
                ReadTask = Task.Run(() => ReadOutputAsync(ReadCancellationSource.Token), cancellationToken);

                return true;
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void ChildProcess(TerminalOptions options)
    {
        try
        {
            close(MasterFileDescription);
            MasterFileDescription = -1;

            if (login_tty(SlaveFileDescription) == -1)
            {
                ExitWithError("login_tty failed");
            }

            // 设置工作目录
            if (!string.IsNullOrEmpty(options.WorkingDirectory))
            {
                if (!Directory.Exists(options.WorkingDirectory))
                {
                    ExitWithError($"Working directory not found: {options.WorkingDirectory}");
                }
                Directory.SetCurrentDirectory(options.WorkingDirectory);
            }

            // 设置环境变量（合并系统环境变量和自定义环境变量）
            SetEnvironmentVariables(options.EnvironmentVariables);

            // 解析 shell 命令
            var shellArgs = ParseShellCommand(options.Shell);
            var shell = shellArgs[0];
            var args = shellArgs.Length > 1 ? shellArgs[1..] : Array.Empty<string>();

            // 准备执行参数
            var execArgs = PrepareExecArgs(shell, args);
            var envVars = PrepareEnvironmentVariables(options.EnvironmentVariables);

            // 执行命令
            execve(execArgs[0], execArgs, envVars);

            // 如果 execve 失败，尝试 fallback
            execve("/bin/sh", new[] { "sh", "-i", null }, envVars);

            ExitWithError("All execve attempts failed");
        }
        catch (Exception ex)
        {
            ExitWithError($"Child process exception: {ex.Message}");
        }
    }

    private string[] ParseShellCommand(string shell)
    {
        if (string.IsNullOrEmpty(shell))
            return new[] { "/bin/sh" };

        var parts = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (char c in shell)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts.Count == 0 ? new[] { "/bin/sh" } : parts.ToArray();
    }

    private void SetEnvironmentVariables(IDictionary<string, string> customVars)
    {
        // 先设置自定义变量
        if (customVars != null)
        {
            foreach (var env in customVars)
            {
                Environment.SetEnvironmentVariable(env.Key, env.Value);
            }
        }

        // 确保有 TERM 环境变量
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM")))
        {
            Environment.SetEnvironmentVariable("TERM", "xterm-256color");
        }
    }

    private string?[] PrepareEnvironmentVariables(IDictionary<string, string> customVars)
    {
        var envVars = new List<string?>();

        // 合并环境变量
        var allVars = new Dictionary<string, string?>();

        // 添加系统环境变量
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            allVars[entry.Key.ToString() ?? throw new NullReferenceException()] = entry.Value?.ToString();
        }

        // 用自定义变量覆盖
        if (customVars != null)
        {
            foreach (var env in customVars)
            {
                allVars[env.Key] = env.Value;
            }
        }

        // 转换为数组
        foreach (var env in allVars)
        {
            if (env.Value != null)
            {
                envVars.Add($"{env.Key}={env.Value}");
            }
        }

        envVars.Add(null); // 必须以 null 结尾
        return envVars.ToArray();
    }

    private string?[] PrepareExecArgs(string shell, string?[] args)
    {
        var execArgs = new List<string?> { shell };
        execArgs.AddRange(args);
        execArgs.Add(null); // 必须以 null 结尾
        return execArgs.ToArray();
    }

    private void ExitWithError(string message)
    {
        // 可以在这里记录错误信息
        Environment.Exit(1);
    }

    /// <summary>
    /// 检查终端是否仍在运行
    /// </summary>
    /// <returns></returns>
    public bool IsRunning()
    {
        if (ChildProcessID <= 0) return false;

        // 检查进程是否还在运行
        if (waitpid(ChildProcessID, out var status, (int)WaitPidFlags.WNOHANG) > 0)
        {
            return false; // 进程已结束
        }
        return true;
    }

    private async Task ReadOutputAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];

        try
        {
            while (!cancellationToken.IsCancellationRequested && MasterStream != null)
            {
                var bytesRead = await MasterStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                {
                    OutputReceived?.Invoke(buffer, bytesRead);
                }
                else
                {
                    // 流已关闭
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 读取被取消，正常退出
        }
        catch (ObjectDisposedException)
        {
            // 流已被释放，正常退出
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"读取终端输出时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 向终端输入数据
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task WriteInputAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
    {
        if (MasterStream == null || !MasterStream.CanWrite)
            throw new InvalidOperationException("终端未启动或不可写");

        await MasterStream.WriteAsync(buffer, 0, length, cancellationToken);
        await MasterStream.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// 调整终端大小
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="rows"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task ResizeAsync(int columns, int rows, CancellationToken cancellationToken = default)
    {
        if (MasterFileDescription == -1)
            throw new InvalidOperationException("终端未启动");

        var winsize = new WinSize
        {
            ws_col = (ushort)columns,
            ws_row = (ushort)rows,
            ws_xpixel = 0,
            ws_ypixel = 0
        };

        const int TIOCSWINSZ = 0x5414; // 设置窗口大小的 IOCTL 命令

        if (ioctl(MasterFileDescription, TIOCSWINSZ, ref winsize) == -1)
        {
            throw new InvalidOperationException($"调整终端大小失败: {Marshal.GetLastWin32Error()}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        try
        {
            // 停止读取
            ReadCancellationSource?.Cancel();

            try
            {
                ReadTask?.Wait(1000);
            }
            catch (AggregateException) { }

            // 终止子进程
            if (ChildProcessID > 0)
            {
                kill(ChildProcessID, 15); // SIGTERM

                // 等待进程结束
                if (waitpid(ChildProcessID, out _, 0) == -1)
                {
                    // 如果正常终止失败，强制杀死
                    kill(ChildProcessID, 9); // SIGKILL
                    waitpid(ChildProcessID, out _, 0);
                }
            }

            // 关闭文件描述符
            if (MasterFileDescription != -1)
            {
                close(MasterFileDescription);
                MasterFileDescription = -1;
            }

            if (SlaveFileDescription != -1)
            {
                close(SlaveFileDescription);
                SlaveFileDescription = -1;
            }

            MasterStream?.Dispose();
            MasterStream = null;
            ReadCancellationSource?.Dispose();
        }
        finally
        {
            IsDisposed = true;
        }
    }
}