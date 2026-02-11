namespace TidyHPC.LiteJson;

/// <summary>
/// Json修复工具
/// </summary>
public static class JsonRepair
{
    /// <summary>
    /// 修复引号
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public static string RepairQuote(string raw)
    {
        // 修复场景
        // 1. {"value":"他说："xxxx""}
        bool isInString = false;
        List<int> quoteIndices = [];
        void enqueueQuoteIndex(int index)
        {
            if (quoteIndices.Count >= 10)
            {
                quoteIndices.RemoveAt(0);
            }
            quoteIndices.Add(index);
        }
        char[] jsonFormatChars = ['{', '}', '[', ']', ':', ',', ' ', '\t', '\r', '\n'];
        for (int charIndex = 0; charIndex < raw.Length; charIndex++)
        {
            var c = raw[charIndex];
            if (c == '"')
            {
                if (isInString)
                {
                    if (charIndex > 0 && raw[charIndex - 1] == '\\')
                    {
                        // 转义引号
                    }
                    else
                    {
                        // 结束引号
                        isInString = false;
                        enqueueQuoteIndex(charIndex);
                    }
                }
                else
                {
                    // 开始引号
                    isInString = true;
                    enqueueQuoteIndex(charIndex);
                }
            }
            else if (isInString == false)
            {
                if (jsonFormatChars.Contains(c) == false)
                {
                    // 非法字符
                    if (quoteIndices.Count > 0)
                    {
                        var lastQuoteIndex = quoteIndices.Last();
                        raw = raw.Insert(lastQuoteIndex, "\\");
                        charIndex++;
                        quoteIndices.Clear();
                        // 寻找下一个引号
                        while (true)
                        {
                            charIndex++;
                            if (charIndex >= raw.Length) break;
                            if (raw[charIndex] == '"')
                            {
                                raw = raw.Insert(charIndex, "\\");
                                charIndex++;
                                isInString = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        return raw;
    }

    /// <summary>
    /// 修复缩进
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public static string RepairIndent(string raw)
    {
        Queue<char> indentQueue = new();
        List<char> result = new(raw.Length);
        char[] indentChars = ['[', ']', '{', '}'];
        bool isInString = false;
        bool isInTranslate = false;
        var charIndex = -1;
        foreach (var c in raw)
        {
            charIndex++;
            if (isInString)
            {
                if (isInTranslate)
                {
                    isInTranslate = false;
                    result.Add(c);
                }
                else if (c == '"')
                {
                    isInString = true;
                    result.Add(c);
                }
                else
                {
                    result.Add(c);
                }
            }
            else if (c == '\"')
            {
                isInString = true;
                result.Add(c);
            }
            else if (indentChars.Contains(c))
            {
                if (c == '[' || c == '{')
                {
                    indentQueue.Enqueue(c);
                    result.Add(c);
                }
                else if (c == ']' || c == '}')
                {
                    var lastIndent = indentQueue.Dequeue();
                    if (lastIndent == '[')
                    {
                        result.Add(']');
                    }
                    else if (lastIndent == '{')
                    {
                        result.Add('}');
                    }
                }
                else
                {
                    result.Add(c);
                }
            }
            else
            {
                result.Add(c);
            }
        }
        while (indentQueue.Count > 0)
        {
            var lastIndent = indentQueue.Dequeue();
            if (lastIndent == '[')
            {
                result.Add(']');
            }
            else if (lastIndent == '{')
            {
                result.Add('}');
            }
        }
        
        return new string(result.ToArray());
    }


}