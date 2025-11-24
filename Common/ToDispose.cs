namespace TidyHPC.Common;

/// <summary>
/// 可释放对象占位符
/// </summary>
public class ToDispose : IDisposable
{
    private readonly List<Action> ToDisposes = [];

    private readonly List<IDisposable> Disposables = [];

    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        foreach (var action in ToDisposes)
        {
            action();
        }
        foreach (var disposable in Disposables)
        {
            disposable.Dispose();
        }
        ToDisposes.Clear();
        Disposables.Clear();
    }

    /// <summary>
    /// 添加释放操作
    /// </summary>
    /// <param name="action"></param>
    public void Add(Action action)
    {
        ToDisposes.Add(action);
    }

    /// <summary>
    /// 添加可释放对象
    /// </summary>
    /// <param name="disposables"></param>
    public void Add(params IDisposable[] disposables)
    {
        Disposables.AddRange(disposables);
    }
}