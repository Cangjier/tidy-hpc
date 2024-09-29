namespace TidyHPC.Routers;

/// <summary>
/// 参数属性
/// </summary>
public class ArgsAliasesAttribute : Attribute
{
    /// <summary>
    /// 选项别名
    /// </summary>
    public string[] Aliases { get; }

    /// <summary>
    /// 参数属性
    /// </summary>
    /// <param name="aliases"></param>
    /// <param name="firstAlias"></param>
    public ArgsAliasesAttribute(string firstAlias, params string[] aliases)
    {
        Aliases = new string[aliases.Length + 1];
        Aliases[0] = firstAlias;
        aliases.CopyTo(Aliases, 1);
    }
}