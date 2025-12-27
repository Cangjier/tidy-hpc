using TidyHPC.Loggers;

namespace TidyHPC.Terminal.Windows;

internal class WindowsTerminal : ITerminal
{
    public WindowsTerminal(TerminalOptions options)
    {
        Options = options;
    }

    public Guid ID { get; set; } = Guid.NewGuid();
    public bool IsRunning => throw new NotImplementedException();

    public event Func<byte[], int, Task>? OutputReceived;
    public event EventHandler<int>? Exited;
    private PseudoConsolePipe? InputPipe = new();
    private PseudoConsolePipe? OutputPipe = new();
    private PseudoConsole? PseudoConsole;
    private Process? Process;
    private Task? OutputTask;
    private FileStream? InputStream;

    public TerminalOptions Options { get; }

    /// <summary>
    /// 输出缓冲区大小
    /// </summary>
    public int OutputBufferSize => Options.OutputBufferSize;

    public void Dispose()
    {
        Process?.Dispose();
        Process = null;
        PseudoConsole?.Dispose();
        PseudoConsole = null;
        InputPipe?.Dispose();
        InputPipe = null;
        OutputPipe?.Dispose();
        OutputPipe = null;
        OutputTask?.Dispose();
        OutputTask = null;
        InputStream?.Dispose();
        InputStream = null;
    }

    public async Task ResizeAsync(int columns, int rows, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        if (PseudoConsole == null)
        {
            throw new InvalidOperationException("PseudoConsole is not initialized.");
        }
        PseudoConsole.Resize((short)columns, (short)rows);
    }

    public async Task<bool> StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        try
        {
            if (InputPipe is null || OutputPipe is null)
            {
                throw new InvalidOperationException("Pipes are not initialized.");
            }
            PseudoConsole = PseudoConsole.Create(InputPipe.ReadSide, OutputPipe.WriteSide, (short)Options.Columns, (short)Options.Rows);
            Process = ProcessFactory.Start(Options.Shell, PseudoConsole.PseudoConsoleThreadAttribute, PseudoConsole.Handle, Options.WorkingDirectory);
            OutputTask = Task.Run(async () =>
            {
                using var pseudoConsoleOutput = new FileStream(OutputPipe.ReadSide, FileAccess.Read);
                var buffer = new byte[Options.OutputBufferSize];
                int bytesRead;
                while ((bytesRead = await pseudoConsoleOutput.ReadAsync(buffer)) > 0)
                {
                    if (OutputReceived is not null)
                    {
                        await OutputReceived(buffer, bytesRead);
                    }
                }
            }, cancellationToken);
            InputStream = new FileStream(InputPipe.WriteSide, FileAccess.Write);
        }
        catch (Exception e)
        {
            Logger.Error(e);
            return false;
        }
        return true;
    }

    public Task WriteInputAsync(byte[] buffer, int length, CancellationToken cancellationToken = default)
    {
        if (InputStream == null)
        {
            throw new InvalidOperationException("Input stream is not initialized.");
        }
        return InputStream.WriteAsync(buffer, 0, length, cancellationToken);
    }
}
