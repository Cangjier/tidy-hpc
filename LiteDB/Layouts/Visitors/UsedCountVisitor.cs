namespace TidyHPC.LiteDB.Layouts.Visitors;

/// <summary>
/// 字段的访问器
/// </summary>
/// <param name="layout"></param>
public class UsedCountVisitor(StatisticalLayout layout)
{
    /// <summary>
    /// 访问的块
    /// </summary>
    public StatisticalLayout Layout { get; } = layout;

    /// <summary>
    /// 读取已使用数量
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Read(Database table, Action<int> onBuffer)
    {
        await LayoutUtil.ProcessReadWithCache(
            table,
            Layout.Address,
            Layout.CacheSize,
            Layout.UsedCountAddress,
            StatisticalLayout.UsedCountSize,
            table.UsedCountSemaphore,
            (buffer, offset) =>
            {
                try
                {
                    onBuffer(BitConverter.ToInt32(buffer, offset));
                }
                catch
                {
                    Console.WriteLine($"""
                        -------------------------
                        offset={offset}
                        {ToString()}
                        -------------------------
                        """);
                    throw;
                }
            });
    }

    /// <summary>
    /// 写入已使用数量
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Write(Database table, Func<int> onBuffer)
    {
        await LayoutUtil.ProcessWriteWithCache(
        table,
        Layout.Address,
        Layout.CacheSize,
        Layout.UsedCountAddress,
        StatisticalLayout.UsedCountSize,
        table.UsedCountSemaphore,
        (buffer, offset) => BitConverter.GetBytes(onBuffer()).CopyTo(buffer, offset));
    }

    /// <summary>
    /// 更新已使用数量
    /// </summary>
    /// <param name="table"></param>
    /// <param name="onBuffer"></param>
    /// <returns></returns>
    public async Task Update(Database table, Func<int, (int, bool)> onBuffer)
    {
        await LayoutUtil.ProcessUpdateWithCache(
        table,
        Layout.Address,
        Layout.CacheSize,
        Layout.UsedCountAddress,
        StatisticalLayout.UsedCountSize,
        table.UsedCountSemaphore, (buffer, offset) =>
        {
            var result = onBuffer(BitConverter.ToInt32(buffer, offset));
            if (result.Item2)
            {
                BitConverter.GetBytes(result.Item1).CopyTo(buffer, offset);
            }
            return result.Item2;
        });
    }

    /// <summary>
    /// 转换成Json String
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $$"""
            {
                "Address":"{{Layout.Address}}",
                "UsedCountAddress":"{{Layout.UsedCountAddress}}",
                "UsedCountSize":"{{StatisticalLayout.UsedCountSize}}",
                "CacheSize":"{{Layout.CacheSize}}",
                "Offset":"{{(int)(Layout.UsedCountAddress - Layout.Address)}}"
            }
            """;
    }

    internal void Check()
    {
        var cacaheOffset = (int)(Layout.UsedCountAddress - Layout.Address);
        if (cacaheOffset < 0 || cacaheOffset >= Layout.CacheSize)
        {
            throw new Exception($"UsedCountVisitor Read Error,`Block.UsedCountAddress - Block.Address` = {Layout.UsedCountAddress - Layout.Address}");
        }
    }

}
