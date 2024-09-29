using System.Text.Json;
using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;
public partial struct Json
{
    #region Implicit
    /// <summary>
    /// convert string to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(string? value)
    {
        return new Json(value);
    }

    /// <summary>
    /// Convert JsonNode to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(JsonNode? value)
    {
        return new(value);
    }

    /// <summary>
    /// Implicit convert JsonObject to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(Dictionary<string,object?>? value) => new(value);

    /// <summary>
    /// Implicit convert JsonArray to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(List<object?>? value) => new(value);

    /// <summary>
    /// Implicit convert JsonElement to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(JsonElement value) => new(value);

    /// <summary>
    /// Convert Json to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(byte value) => new(value);

    /// <summary>
    /// Convert byte to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(byte? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert short to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(sbyte value) => new(value);

    /// <summary>
    /// Convert sbyte to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(sbyte? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert short to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(short value) => new(value);

    /// <summary>
    /// Convert short to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(short? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert ushort to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(ushort value) => new(value);

    /// <summary>
    /// Convert ushort to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(ushort? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert int to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(int value) => new(value);

    /// <summary>
    /// Convert int to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(int? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert uint to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(uint value) => new(value);

    /// <summary>
    /// Convert uint to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(uint? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert long to JsonNode
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(long value) => new(value);

    /// <summary>
    /// Convert long to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(long? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert ulong to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(ulong value) => new(value);

    /// <summary>
    /// Convert ulong to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(ulong? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert double to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(double value) => new(value);

    /// <summary>
    /// Convert double to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(double? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert decimal to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(decimal value) => new(value);

    /// <summary>
    /// Convert decimal to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(decimal? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert bool to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(bool value) => new(value);

    /// <summary>
    /// Convert bool to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(bool? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value);
        }
    }

    /// <summary>
    /// Convert DateTime to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(DateTime value) => new(value.ToString("O"));

    /// <summary>
    /// Convert DateTime to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(DateTime? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value.ToString("O"));
        }
    }

    /// <summary>
    /// Convert TimeSpan to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(TimeSpan value) => new(value.ToString());

    /// <summary>
    /// Convert TimeSpan to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(TimeSpan? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value.ToString());
        }
    }

    /// <summary>
    /// Convert Guid to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(Guid value) => new(value.ToString());

    /// <summary>
    /// Convert Guid to Json
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Json(Guid? value)
    {
        if (value == null)
        {
            return Null;
        }
        else
        {
            return new(value.Value.ToString());
        }
    }

    /// <summary>
    /// 隐式转换代理，用于Script
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Json op_ImplicitFrom(object value)
    {
        return new(value);
    }

    /// <summary>
    /// 隐式转换代理，用于Script
    /// </summary>
    /// <param name="value"></param>
    /// <param name="toType"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static object? op_ImplicitTo(Json value,Type toType)
    {
        if (toType == typeof(string) && value.IsString == false) return value.ToString();
        else if (toType == typeof(int) && value.IsInt32 == false) return value.ToInt32;
        else if (toType == typeof(string[]))
        {
            return value.ToArray(item => item.AsString);
        }
        else if (toType == typeof(int[]))
        {
            return value.ToArray(item => item.ToInt32);
        }
        else return value.Node;
    }
    #endregion
}
