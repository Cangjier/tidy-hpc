using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;
internal static class JsonUtil
{
    public static bool IsNumber(object? value)
    {
        return value is byte || value is short || value is int || value is long || value is float || value is double || value is decimal;
    }

    public static bool DeepEquals(object? a,object? b)
    {
        if(a== null && b == null)
        {
            return true;
        }
        if(a == null || b == null)
        {
            return false;
        }
        // 如果a和b都是number类型
        if (IsNumber(a))
        {
            if (IsNumber(b))
            {
                return Convert.ToDecimal(a) == Convert.ToDecimal(b);
            }
            return false;
        }
        return a.Equals(b) || b.Equals(a);
    }

    public static JsonNode? ToJsonNode(this object? self)
    {
        if (self is JsonNode jsonNode) return jsonNode;
        else if (self is null) return null;
        else if (self is bool valueBoolean) return valueBoolean;
        else if (self is double valueDouble) return valueDouble;
        else if (self is int valueInt) return valueInt;
        else if (self is long valueLong) return valueLong;
        else if (self is string valueString) return valueString;
        else if (self is Json valueJson) return valueJson.Node.ToJsonNode();
        else if (self is JsonArray valueJsonArray) return valueJsonArray;
        else if (self is JsonObject valueJsonObject) return valueJsonObject;
        else 
            throw new NotImplementedException();
    }
}
