﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;
public partial struct Json
{
    #region Is As
    /// <summary>
    /// Is Object
    /// </summary>
    public readonly bool IsObject => GetValueKind() == JsonValueKind.Object;

    /// <summary>
    /// As Object
    /// </summary>
    public readonly ObjectWrapper AsObject => new(Node);

    /// <summary>
    /// Is Array
    /// </summary>
    public readonly bool IsArray => GetValueKind() == JsonValueKind.Array;

    /// <summary>
    /// As Array
    /// </summary>
    public readonly ArrayWrapper AsArray => new(Node);

    /// <summary>
    /// Is String
    /// </summary>
    public readonly bool IsString => GetValueKind() == JsonValueKind.String;

    /// <summary>
    /// Is String Predicate
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public readonly bool IsStringPredicate(Func<string, bool> predicate) => IsString && predicate(AsString);

    /// <summary>
    /// If self is String, get the value of String
    /// </summary>
    public readonly string AsString
    {
        get
        {
            if (Node is JsonNode jsonNode) return jsonNode.GetValue<string>();
            else if (Node is string nodeString) return nodeString;
            else if (Node is char nodeChar) return nodeChar.ToString();
            else if (Node is DateTime dateTime) return dateTime.ToString("O");
            else if (Node is Guid guid) return guid.ToString();
            throw new Exception("Can't convert to string");
        }
    }

    /// <summary>
    /// Is Number
    /// </summary>
    public readonly bool IsNumber => GetValueKind() == JsonValueKind.Number;

    /// <summary>
    /// If self is Number, get the value of Number
    /// </summary>
    public readonly double AsNumber
    {
        get
        {
            if (Node == null) throw new Exception("Node is null");
            if (Node is JsonNode jsonNode)
            {
                var value = jsonNode.AsValue();
                if (value.TryGetValue(out int intValue))
                {
                    return intValue;
                }
                else if (value.TryGetValue(out long longValue))
                {
                    return longValue;
                }
                else if (value.TryGetValue(out float floatValue))
                {
                    return floatValue;
                }
                else if (value.TryGetValue(out double doubleValue))
                {
                    return doubleValue;
                }
                else if (value.TryGetValue(out decimal decimalValue))
                {
                    return (double)decimalValue;
                }
                else if (value.TryGetValue(out byte byteValue))
                {
                    return byteValue;
                }
                else
                {
                    return 0;
                }
            }
            else if (Node is double nodeDouble) return nodeDouble;
            else if (Node is int nodeInt) return nodeInt;
            else if (Node is long nodeLong) return nodeLong;
            else if (Node is float nodeFloat) return nodeFloat;
            else if (Node is decimal nodeDecimal) return (double)nodeDecimal;
            else if (Node is byte nodeByte) return nodeByte;
            else if (Node is short nodeShort) return nodeShort;
            else if (Node is sbyte nodeSByte) return nodeSByte;
            else if (Node is ushort nodeUShort) return nodeUShort;
            else if (Node is uint nodeUInt) return nodeUInt;
            else if (Node is ulong nodeULong) return nodeULong;
            else if (Node is string nodeString) return double.Parse(nodeString);
            else if (Node is char nodeChar) return nodeChar;
            else throw new Exception($"Can't convert to number, {Node?.GetType()}");
        }
    }

    /// <summary>
    /// Is Value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public readonly bool Is<T>()
    {
        if (Node is T) return true;
        var tType = typeof(T);
        if (Node is JsonNode jsonNode)
        {
            if (tType == typeof(string)) if (jsonNode.GetValueKind() == JsonValueKind.String) return true;
            if (tType == typeof(int)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(long)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(float)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(double)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(decimal)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(byte)) if (jsonNode.GetValueKind() == JsonValueKind.Number && jsonNode.AsValue().TryGetValue(out T? _)) return true;
            if (tType == typeof(bool)) if (jsonNode.GetValueKind() == JsonValueKind.True || jsonNode.GetValueKind() == JsonValueKind.False) return true;
            if (tType == typeof(Guid)) if (jsonNode.GetValueKind() == JsonValueKind.String && Guid.TryParse(jsonNode.GetValue<string>(), out _)) return true;
            if(tType == typeof(DateTime)) if (jsonNode.GetValueKind() == JsonValueKind.String && DateTime.TryParse(jsonNode.GetValue<string>(), out _)) return true;
            if (tType == typeof(TimeSpan)) if (jsonNode.GetValueKind() == JsonValueKind.String && TimeSpan.TryParse(jsonNode.GetValue<string>(), out _)) return true;
        }
        else if(Node is string nodeString)
        {
            if(tType == typeof(Guid)) if (Guid.TryParse(nodeString, out _)) return true;
            if (tType == typeof(DateTime)) if (DateTime.TryParse(nodeString, out _)) return true;
            if (tType == typeof(TimeSpan)) if (TimeSpan.TryParse(nodeString, out _)) return true;
            if (tType == typeof(Uri)) if (Uri.TryCreate(nodeString, UriKind.RelativeOrAbsolute, out _)) return true;
        }
        return false;
    }

    /// <summary>
    /// As Value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public readonly T As<T>()
    {
        if (Node is T nodeT) return nodeT;
        var tType = typeof(T);
        if (Node is JsonNode jsonNode)
        {
            if (tType == typeof(string)) return (T)(object)jsonNode.GetValue<string>();
            if (tType == typeof(int)) return (T)(object)jsonNode.GetValue<int>();
            if (tType == typeof(long)) return (T)(object)jsonNode.GetValue<long>();
            if (tType == typeof(float)) return (T)(object)jsonNode.GetValue<float>();
            if (tType == typeof(double)) return (T)(object)jsonNode.GetValue<double>();
            if (tType == typeof(decimal)) return (T)(object)jsonNode.GetValue<decimal>();
            if (tType == typeof(byte)) return (T)(object)jsonNode.GetValue<byte>();
            if (tType == typeof(bool)) return (T)(object)jsonNode.GetValue<bool>();
            if (tType == typeof(Guid)) return (T)(object)Guid.Parse(jsonNode.GetValue<string>());
        }
        if (Node is string nodeString)
        {
            if (tType == typeof(Guid)) return (T)(object)Guid.Parse(nodeString);
            if (tType == typeof(DateTime)) return (T)(object)DateTime.Parse(nodeString);
            if (tType == typeof(TimeSpan)) return (T)(object)TimeSpan.Parse(nodeString);
            if (tType == typeof(Uri)) return (T)(object)new Uri(nodeString);
        }
        if (tType == typeof(int))
        {
            if (Node is Int16 nodeInt16) return (T)(object)(int)nodeInt16;
            if (Node is byte nodeByte) return (T)(object)(int)nodeByte;
            if (Node is double nodeDouble) return (T)(object)(int)nodeDouble;
            if (Node is long nodeLong) return (T)(object)(int)nodeLong;
            if (Node is float nodeFloat) return (T)(object)(int)nodeFloat;
            if (Node is decimal nodeDecimal) return (T)(object)(int)nodeDecimal;
        }
        if (tType == typeof(long))
        {
            if (Node is Int16 nodeInt16) return (T)(object)(long)nodeInt16;
            if (Node is byte nodeByte) return (T)(object)(long)nodeByte;
            if (Node is double nodeDouble) return (T)(object)(long)nodeDouble;
            if (Node is int nodeInt) return (T)(object)(long)nodeInt;
            if (Node is float nodeFloat) return (T)(object)(long)nodeFloat;
            if (Node is decimal nodeDecimal) return (T)(object)(long)nodeDecimal;
        }
        if (tType == typeof(float))
        {
            if (Node is Int16 nodeInt16) return (T)(object)(float)nodeInt16;
            if (Node is byte nodeByte) return (T)(object)(float)nodeByte;
            if (Node is double nodeDouble) return (T)(object)(float)nodeDouble;
            if (Node is int nodeInt) return (T)(object)(float)nodeInt;
            if (Node is long nodeLong) return (T)(object)(float)nodeLong;
            if (Node is decimal nodeDecimal) return (T)(object)(float)nodeDecimal;
        }
        if (tType == typeof(double))
        {
            if (Node is Int16 nodeInt16) return (T)(object)(double)nodeInt16;
            if (Node is byte nodeByte) return (T)(object)(double)nodeByte;
            if (Node is float nodeFloat) return (T)(object)(double)nodeFloat;
            if (Node is int nodeInt) return (T)(object)(double)nodeInt;
            if (Node is long nodeLong) return (T)(object)(double)nodeLong;
            if (Node is decimal nodeDecimal) return (T)(object)(double)nodeDecimal;
        }


        throw new Exception($"Can't convert {Node?.GetType().Name} to " + tType.Name);
    }

    /// <summary>
    /// Is Byte
    /// </summary>
    public readonly bool IsByte => Is<byte>();

    /// <summary>
    /// If self is Number, get the value of Number as byte
    /// </summary>
    public readonly byte AsByte => As<byte>();

    /// <summary>
    /// Is Int32
    /// </summary>
    public readonly bool IsInt32 => Is<Int32>();

    /// <summary>
    /// If self is Number, get the value of Number as int
    /// </summary>
    public readonly int AsInt32 => As<Int32>();

    /// <summary>
    /// Convert to Int32
    /// </summary>
    public readonly int ToInt32
    {
        get
        {
            if (IsString)
            {
                return int.Parse(AsString);
            }
            else if (IsInt32) return AsInt32;
            else if (IsNumber)
            {
                return (int)AsNumber;
            }
            else
            {
                throw new Exception("Can't convert to Int32");
            }
        }

    }


    /// <summary>
    /// Is Int64
    /// </summary>
    public readonly bool IsInt64 => Is<Int64>();

    /// <summary>
    /// If self is Number, get the value of Number as long
    /// </summary>
    public readonly long AsInt64 => As<Int64>();

    /// <summary>
    /// Is Float
    /// </summary>
    public readonly bool IsFloat => Is<float>();

    /// <summary>
    /// If self is Number, get the value of Number as float
    /// </summary>
    public readonly float AsFloat => As<float>();

    /// <summary>
    /// Convert to Float
    /// </summary>
    public readonly float ToFloat
    {
        get
        {
            if (IsString)
            {
                return float.Parse(AsString);
            }
            else if (IsInt32) return AsInt32;
            else if (IsNumber)
            {
                return (float)AsNumber;
            }
            else
            {
                throw new Exception("Can't convert to Float");
            }
        }

    }

    /// <summary>
    /// Is Double
    /// </summary>
    public readonly bool IsDouble => Is<double>();

    /// <summary>
    /// If self is Number, get the value of Number as decimal
    /// </summary>
    public readonly double AsDouble => As<double>();

    /// <summary>
    /// Convert to Double
    /// </summary>
    public readonly double ToDouble
    {
        get
        {
            if (IsString)
            {
                return double.Parse(AsString);
            }
            else if (IsNumber)
            {
                return AsNumber;
            }
            else
            {
                throw new Exception("Can't convert to Float32");
            }
        }

    }

    /// <summary>
    /// Is Boolean
    /// </summary>
    public readonly bool IsBoolean => Is<bool>();

    /// <summary>
    /// If self is Boolean, get the value of Boolean
    /// </summary>
    public readonly bool AsBoolean => As<bool>();

    /// <summary>
    /// Is True
    /// </summary>
    public readonly bool IsTrue => GetValueKind() == JsonValueKind.True;

    /// <summary>
    /// Is False
    /// </summary>
    public readonly bool IsFalse => GetValueKind() == JsonValueKind.False;

    /// <summary>
    /// Is Guid
    /// </summary>
    public readonly bool IsGuid => Is<Guid>();

    /// <summary>
    /// As Guid
    /// </summary>
    public readonly Guid AsGuid => As<Guid>();

    /// <summary>
    /// Is DateTime
    /// </summary>
    public readonly bool IsDateTime => Is<DateTime>();

    /// <summary>
    /// As DateTime
    /// </summary>
    public readonly DateTime AsDateTime => As<DateTime>();

    /// <summary>
    /// Is TimeSpan
    /// </summary>
    public readonly bool IsTimeSpan => Is<TimeSpan>();

    /// <summary>
    /// As TimeSpan
    /// </summary>
    public readonly TimeSpan AsTimeSpan => As<TimeSpan>();

    /// <summary>
    /// Is Null
    /// </summary>
    public readonly bool IsNull => Node == null || GetValueKind() == JsonValueKind.Null;

    /// <summary>
    /// Is undefined
    /// </summary>
    public readonly bool IsUndefined => IsString && AsString == "undefined-622102F6-FF98-4CB6-887B-175F4C1024B0";

    #endregion
}
