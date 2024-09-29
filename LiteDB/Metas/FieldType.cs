namespace TidyHPC.LiteDB.Metas;

/// <summary>
/// 字段类型
/// </summary>
public enum FieldType:byte
{
    /// <summary>
    /// 比特
    /// </summary>
    Byte,
    /// <summary>
    /// True or False
    /// </summary>
    Boolean,
    /// <summary>
    /// 字符
    /// </summary>
    Char,
    /// <summary>
    /// 32位整数
    /// </summary>
    Int32,
    /// <summary>
    /// 浮点数
    /// </summary>
    Float,
    /// <summary>
    /// 双精度浮点数
    /// </summary>
    Double,
    /// <summary>
    /// 64位整数
    /// </summary>
    Int64,
    /// <summary>
    /// 引用字符串
    /// </summary>
    ReferneceString,
    /// <summary>
    /// Guid
    /// </summary>
    Guid,
    /// <summary>
    /// MD5
    /// </summary>
    MD5,
    /// <summary>
    /// DateTime
    /// </summary>
    DateTime,
}

/// <summary>
/// FieldType扩展
/// </summary>
public static class FieldTypeExtension
{
    /// <summary>
    /// Get Size
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static int GetSize(this FieldType type)
    {
        switch (type)
        {
            case FieldType.Byte:
                return sizeof(byte);
            case FieldType.Boolean:
                return sizeof(bool);
            case FieldType.Int32:
                return sizeof(int);
            case FieldType.Float:
                return sizeof(float);
            case FieldType.Double:
                return sizeof(double);
            case FieldType.Char:
                return sizeof(byte);
            case FieldType.ReferneceString:
                return sizeof(long);
            case FieldType.Guid:
                return 16;
            case FieldType.MD5:
                return 16;
            case FieldType.DateTime:
                return sizeof(long);
            case FieldType.Int64:
                return sizeof(long);
            default:
                throw new Exception("未知类型");
        }
    }
}
