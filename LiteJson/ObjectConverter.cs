using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Diagnostics;

namespace TidyHPC.LiteJson;

/// <summary>
/// Object converter
/// </summary>
public class ObjectConverter : JsonConverter<object?>
{
    /// <inheritdoc/>
    public override object? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            List<object?> result = [];
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    result.Add(JsonSerializer.Deserialize<object>(ref reader, options));
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                { 
                    result.Add(JsonSerializer.Deserialize<object>(ref reader, options));
                }
                else
                {
                    result.Add(JsonSerializer.Deserialize<object>(ref reader, options));
                }
            }
            return result;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            Dictionary<string,object?> result = [];
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                string? propertyName = reader.GetString();
                if (propertyName == null)
                {
                    throw new JsonException();
                }
                reader.Read();
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    result[propertyName] = JsonSerializer.Deserialize<object>(ref reader, options);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    result[propertyName] = JsonSerializer.Deserialize<object>(ref reader, options);
                }
                else
                {
                    result[propertyName] = JsonSerializer.Deserialize<object>(ref reader, options);
                }
            }
            return result;
        }
        else if(reader.TokenType == JsonTokenType.False)
        {
            return false;
        }
        else if(reader.TokenType == JsonTokenType.True)
        {
            return true;
        }
        else if(reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetByte(out byte valueByte))
            {
                return valueByte;
            }
            else if (reader.TryGetInt16(out short valueShort))
            {
                return valueShort;
            }
            else if (reader.TryGetInt32(out int valueInt))
            {
                return valueInt;
            }
            else if (reader.TryGetInt64(out long valueLong))
            {
                return valueLong;
            }
            else if (reader.TryGetSingle(out float valueFloat))
            {
                return valueFloat;
            }
            else if (reader.TryGetDouble(out double valueDouble))
            {
                return valueDouble;
            }
            else
            {
                throw new JsonException();
            }
        }
        else if(reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        else if(reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }
        else if(reader.TokenType == JsonTokenType.Comment)
        {
            return null;
        }
        else
        {
            throw new JsonException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Type converter
/// </summary>
public class UnsupportedConverter : JsonConverter<object>
{
    /// <summary>
    /// Can convert
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeof(Type).IsAssignableFrom(typeToConvert) || typeToConvert == typeof(Json))
        {
            return true;
        }
        else if (typeof(System.Reflection.MemberInfo).IsAssignableFrom(typeToConvert))
        {
            return true;
        }
        // 判断typeToConvert是否为Enum类型
        else if (typeToConvert.IsEnum)
        {
            return true;
        }
        else if (typeToConvert == typeof(Encoding)||typeToConvert==typeof(Process))
        {
            return true;
        }
        else if (typeToConvert.BaseType == typeof(MulticastDelegate))
        {
            return true;
        }
        else if (typeToConvert.FullName?.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder") == true)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Read
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Write
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is Type type)
            writer.WriteStringValue(type.FullName);
        else if (value is MulticastDelegate delegateValue)
            writer.WriteStringValue("[method]");
        else if (value is Json json)
            JsonSerializer.Serialize(writer, json.Node, options);
        else if (value is System.Reflection.MemberInfo member)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", member.Name);
            writer.WriteString("DeclaringType", member.DeclaringType?.FullName);
            writer.WriteString("MemberType", member.MemberType.ToString());
            if (member.MemberType == System.Reflection.MemberTypes.Field)
            {
                var field = (System.Reflection.FieldInfo)member;
                writer.WriteString("FieldType", field.FieldType.FullName);
            }
            else if (member.MemberType == System.Reflection.MemberTypes.Property)
            {
                var property = (System.Reflection.PropertyInfo)member;
                writer.WriteString("PropertyType", property.PropertyType.FullName);
            }
            else if (member.MemberType == System.Reflection.MemberTypes.Method)
            {
                var method = (System.Reflection.MethodInfo)member;
                writer.WriteString("ReturnType", method.ReturnType.FullName);
                writer.WriteStartArray("Parameters");
                foreach (var parameter in method.GetParameters())
                {
                    writer.WriteStartObject();
                    writer.WriteString("Name", parameter.Name);
                    writer.WriteString("ParameterType", parameter.ParameterType.FullName);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
        else if (value is Enum enumValue)
        {
            writer.WriteStringValue(enumValue.ToString());
        }
        else if (value is Encoding encoding)
        {
            writer.WriteStringValue(encoding.WebName);
        }
        else if (value is Process valueProcess)
        {
            writer.WriteStartObject();
            writer.WriteString("ProcessName", valueProcess.ProcessName);
            writer.WriteNumber("Id", valueProcess.Id);
            try
            {
                writer.WriteNumber("TotalProcessorTime", valueProcess.TotalProcessorTime.TotalMilliseconds);
                writer.WriteNumber("WorkingSet64", valueProcess.WorkingSet64);
            }
            catch
            {

            }
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}