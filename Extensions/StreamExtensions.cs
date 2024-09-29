namespace TidyHPC.Extensions;

/// <summary>
/// 流扩展
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 通过滑动窗口读取文件流，匹配指定字节数组，完全匹配则返回当前位置
    /// </summary>
    /// <param name="bytesToFind"></param>
    /// <param name="self"></param>
    /// <returns></returns>
    public static async Task<long> FindBytesAsync(this Stream self,byte[] bytesToFind)
    {
        if (self == null)
            throw new ArgumentNullException(nameof(self));
        if (bytesToFind == null)
            throw new ArgumentNullException(nameof(bytesToFind));
        if (bytesToFind.Length == 0)
            throw new ArgumentException("Bytes to find is empty", nameof(bytesToFind));
        const int bufferSize = 4096; // 缓冲区大小  
        var buffer = new byte[bufferSize];
        int bufferLength = 0; //buffer中实际读取的字节数
        long position = self.Position;  //当前buffer在文件中的位置
        int keepIndex = 0;  //buffer中保留的字节开始索引
        while (true)
        {
            int cachedLength = bufferLength - keepIndex;
            
            if (cachedLength > 0)
            {
                // 将尾部保留的数据移到缓冲区头部
                Array.Copy(buffer, keepIndex, buffer, 0, cachedLength);
            }

            // 读取新数据到缓冲区  
            var readLength = await self.ReadAsync(buffer, cachedLength, bufferSize - cachedLength);
            if (readLength == 0)
                break; // 没有更多的数据可读，退出循环  

            bufferLength = cachedLength + readLength;

            // 在缓冲区中搜索目标字节数组  
            for (int i = 0; i <= bufferLength - bytesToFind.Length; i++)
            {
                if (buffer.AsSpan(i, bytesToFind.Length).SequenceEqual(bytesToFind))
                    return position + i; // 返回匹配的位置  
            }

            // 如果缓冲区末尾的数据不足以进行下一次匹配检查，则保留这些数据  
            if (bufferLength >= bytesToFind.Length)
            {
                keepIndex = bufferLength - bytesToFind.Length;
            }
            else
            {
                keepIndex = 0; // 保留所有读取的数据，因为还不够长以进行匹配  
            }
            position += keepIndex;
        }
        return -1; // 没有找到匹配项  
    }
}
