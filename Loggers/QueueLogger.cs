using TidyHPC.Queues;

namespace TidyHPC.Loggers;

/// <summary>
/// 队列日志
/// </summary>
public class QueueLogger:IDisposable
{
    /// <summary>
    /// Constructor
    /// </summary>
    public QueueLogger() : this(Path.Combine(Path.GetTempPath(),$"{Path.GetFileNameWithoutExtension(Environment.ProcessPath)}-{Guid.NewGuid()}.log"))
    {

    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filePath"></param>
    public QueueLogger(string filePath)
    {
        FilePath = filePath;
        LinesQueue.AddProcessor(async line =>
        {
            await File.AppendAllLinesAsync(FilePath, [line]);
            if (OnLine != null)
            {
                await OnLine(line);
            }
        });
        Task.Run(LinesQueue.Start);
    }

    /// <summary>
    /// 日志写入路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 当有新行时
    /// </summary>
    public Func<string,Task>? OnLine { get; set; }

    private TaskProcessorQueue<string> LinesQueue { get; } = new();

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        LinesQueue.Cancel();
    }

    /// <summary>
    /// 写入一行
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(string line) => LinesQueue.Enqueue(line);
}
