using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using TidyHPC.LiteJson;

namespace TidyHPC;
/// <summary>
/// 工具
/// </summary>
public class Util
{
    /// <summary>
    /// UTF8 without BOM
    /// </summary>
    public static Encoding UTF8 { get; } = new UTF8Encoding(false);

   

    /// <summary>
    /// Hex string to bytes
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte[] HexToBytes(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string must have an even number of characters");
        }

        byte[] bytes = new byte[hexString.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            string byteValue = hexString.Substring(i * 2, 2);
            bytes[i] = byte.Parse(byteValue, NumberStyles.HexNumber);
        }
        return bytes;
    }

    /// <summary>
    /// Hex string to bytes
    /// </summary>
    /// <param name="hexString"></param>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static bool HexToBytes(string hexString, [NotNullWhen(true)]out byte[]? bytes)
    {
        try
        {
            bytes = HexToBytes(hexString);
            return true;
        }
        catch
        {
            bytes = null;
            return false;
        }
    }

    /// <summary>
    /// Bytes to hex string
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string BytesToHexString(byte[] bytes)
    {
        StringBuilder hex = new(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }



}
