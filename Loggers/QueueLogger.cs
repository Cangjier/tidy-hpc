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
            if (EnableWriteToStandardOutpuStream)
            {
                Console.WriteLine(line);
            }
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
    /// 是否写入标准输出流
    /// </summary>
    public bool EnableWriteToStandardOutpuStream { get; set; } = false;

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

    /// <summary>
    /// 等待日志清空
    /// </summary>
    /// <returns></returns>
    public async Task WaitForEmpty()
    {
        while (LinesQueue.BlockingTaskCount > 0)
        {
            await Task.Delay(100);
        }
    }
}
