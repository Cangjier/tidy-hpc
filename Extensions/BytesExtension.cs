namespace TidyHPC.Extensions;

/// <summary>
/// byte扩展方法
/// </summary>
public static class BytesExtensions
{
    /// <summary>
    /// 序列化到缓冲区中
    /// </summary>
    /// <param name="self"></param>
    /// <param name="value"></param>
    /// <param name="offset"></param>
    /// <param name="fixLength"></param>
    /// <returns></returns>
    public static byte[] SerializeRef(this byte[] self, string value, ref int offset,int fixLength)
    {
        var temp = Util.UTF8.GetBytes(value);
        temp.CopyTo(self, offset);
        if (temp.Length > fixLength)
        {
            throw new Exception($"string length is too long: {temp.Length}");
        }
        else if (temp.Length == fixLength)
        {
            offset += fixLength;
        }
        else
        {
            self[offset+temp.Length] = 0;
            offset += fixLength;
        }
        return self;
    }

    /// <summary>
    /// 序列化到缓冲区中
    /// </summary>
    /// <param name="self"></param>
    /// <param name="value"></param>
    /// <param name="offset"></param>
    /// <param name="fixLength"></param>
    /// <returns></returns>
    public static int Serialize(this byte[] self, string value, int offset, int fixLength)
    {
        var temp = Util.UTF8.GetBytes(value);
        temp.CopyTo(self, offset);
        if (temp.Length > fixLength)
        {
            throw new Exception($"string length is too long: {temp.Length}");
        }
        else if (temp.Length == fixLength)
        {

        }
        else
        {
            self[offset + temp.Length] = 0;
        }
        return fixLength;
    }

    /// <summary>
    /// 从缓冲区中反序列化
    /// </summary>
    /// <param name="self"></param>
    /// <param name="offset"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string DeserializeString(this byte[] self, ref int offset,int maxLength)
    {
        return DeserializeStringUnsafe(self, ref offset, maxLength);
    }

    /// <summary>
    /// 从缓冲区中反序列化
    /// </summary>
    /// <param name="self"></param>
    /// <param name="offset"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string DeserializeString(this byte[] self, int offset, int maxLength)
    {
        return DeserializeStringUnsafe(self, offset, maxLength);
    }

    /// <summary>
    /// 从缓冲区中反序列化
    /// </summary>
    /// <param name="self"></param>
    /// <param name="offset"></param>
    /// <param name="fixLength"></param>
    /// <returns></returns>
    public static unsafe string DeserializeStringUnsafe(this byte[] self, ref int offset,int fixLength)
    {
        // 假设offset总是有效的，并且数组中至少有一个0字节作为终止符  
        fixed (byte* pBytes = self)
        {
            byte* ptr = pBytes + offset;

            // 寻找空字节作为终止符  
            int count = 0;
            if (fixLength > 0)
            {
                while (*(ptr + count) != 0 && count < fixLength)
                {
                    count++;
                }
            }
            else
            {
                while (*(ptr + count) != 0)
                {
                    count++;
                }
            }
           

            // 读取字符串  
            string value = new((sbyte*)ptr, 0, count, Util.UTF8);

            // 更新offset，跳过整个字符串包括终止符
            if (fixLength > 0)
            {
                offset += fixLength;
            }
            else
            {
                offset += count + 1;
            }
            return value;
        }
    }

    /// <summary>
    /// 从缓冲区中反序列化
    /// </summary>
    /// <param name="self"></param>
    /// <param name="offset"></param>
    /// <param name="fixLength"></param>
    /// <returns></returns>
    public static unsafe string DeserializeStringUnsafe(this byte[] self, int offset, int fixLength)
    {
        // 假设offset总是有效的，并且数组中至少有一个0字节作为终止符  
        fixed (byte* pBytes = self)
        {
            byte* ptr = pBytes + offset;

            // 寻找空字节作为终止符  
            int count = 0;
            if (fixLength > 0)
            {
                while (*(ptr + count) != 0 && count < fixLength)
                {
                    count++;
                }
            }
            else
            {
                while (*(ptr + count) != 0)
                {
                    count++;
                }
            }


            // 读取字符串  
            string value = new((sbyte*)ptr, 0, count, Util.UTF8);

            // 更新offset，跳过整个字符串包括终止符
            if (fixLength > 0)
            {
                offset += fixLength;
            }
            else
            {
                offset += count + 1;
            }
            return value;
        }
    }
}