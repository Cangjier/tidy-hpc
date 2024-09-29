using System.Text;

namespace TidyHPC.Extensions;

/// <summary>
/// Char Extensions
/// </summary>
public static class CharExtensions
{
    /// <summary>
    /// Repeat char
    /// </summary>
    /// <param name="This"></param>
    /// <param name="Count"></param>
    /// <returns></returns>
    public static string Repeat(this char This, int Count)
    {
        StringBuilder Tmp = new();
        for (int i = 0; i < Count; i++)
        {
            Tmp.Append(This);
        }
        return Tmp.ToString();
    }
}
