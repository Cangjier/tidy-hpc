using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;

/// <summary>
/// Json Extensions
/// </summary>
public static class JsonExtensions
{
#if NET6_0
    /// <summary>
    /// Get the value kind of the node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static System.Text.Json.JsonValueKind GetValueKind(this JsonNode node)
    {
        if (node is JsonObject) return System.Text.Json.JsonValueKind.Object;
        else if (node is JsonArray) return System.Text.Json.JsonValueKind.Array;
        else if (node is JsonValue value)
        {
            var val = value.GetValue<object>();
            if (val == null) return System.Text.Json.JsonValueKind.Null;
            else if (val is string) return System.Text.Json.JsonValueKind.String;
            else if (val is bool) return System.Text.Json.JsonValueKind.True; // or False
            else if (val is int || val is long || val is float || val is double || val is decimal) return System.Text.Json.JsonValueKind.Number;
            else return System.Text.Json.JsonValueKind.Undefined;
        }
        else
        {
            return System.Text.Json.JsonValueKind.Undefined;
        }
    }

    /// <summary>
    /// Deep clone the JsonNode
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static JsonNode DeepClone(this JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var newObj = new JsonObject();
            foreach (var kvp in obj)
            {
                newObj[kvp.Key] = kvp.Value?.DeepClone();
            }
            return newObj;
        }
        else if (node is JsonArray arr)
        {
            var newArr = new JsonArray();
            foreach (var item in arr)
            {
                newArr.Add(item?.DeepClone());
            }
            return newArr;
        }
        else if (node is JsonValue val)
        {
            var value = val.GetValue<object>();
            return JsonValue.Create(value)!;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Deep equals two JsonNode
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    public static bool DeepEquals(this JsonNode? node1, JsonNode? node2)
    {
        if (node1 == null && node2 == null) return true;
        if (node1 == null || node2 == null) return false;
        if (node1.GetValueKind() != node2.GetValueKind()) return false;
        if (node1 is JsonObject obj1 && node2 is JsonObject obj2)
        {
            if (obj1.Count != obj2.Count) return false;
            foreach (var kvp in obj1)
            {
                if (!obj2.ContainsKey(kvp.Key)) return false;
                if (!kvp.Value.DeepEquals(obj2[kvp.Key])) return false;
            }
            return true;
        }
        else if (node1 is JsonArray arr1 && node2 is JsonArray arr2)
        {
            if (arr1.Count != arr2.Count) return false;
            for (int i = 0; i < arr1.Count; i++)
            {
                if (!arr1[i].DeepEquals(arr2[i])) return false;
            }
            return true;
        }
        else if (node1 is JsonValue val1 && node2 is JsonValue val2)
        {
            var value1 = val1.GetValue<object>();
            var value2 = val2.GetValue<object>();
            return Equals(value1, value2);
        }
        else
        {
            return false;
        }
    }
#endif

    /// <summary>
    /// Check if the json is an array
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsObject(this JsonNode? node)
    {
        if (node == null) return false;
        return node.GetValueKind() == System.Text.Json.JsonValueKind.Object;
    }

    /// <summary>
    /// Check if the json is an array
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool IsArray(this JsonNode? node)
    {
        if (node == null) return false;
        return node.GetValueKind() == System.Text.Json.JsonValueKind.Array;
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    public static void EachAll(this Json self,Action<Json> onValue)
    {
        if (self.IsArray)
        {
            foreach (var item in self)
            {
                EachAll(item, onValue);
            }
        }
        else if (self.IsObject)
        {
            foreach(var item in self.GetObjectEnumerable())
            {
                EachAll(item.Value, onValue);
            }
        }
        else
        {
            onValue(self);
        }
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    public static Json EachAll(this Json self,Func<Json,Json> onValue)
    {
        if (self.IsArray)
        {
            var array = self.AsArray;
            int count = array.Count;
            for (var i = 0; i < count; i++)
            {
                array[i] = EachAll(new(array[i]), onValue).Node;
            }
            return self;
        }
        else if (self.IsObject)
        {
            var obj = self.AsObject;
            var lst = obj.ToArray();
            foreach(var item in lst)
            {
                obj[item.Key] = EachAll(new(item.Value), onValue).Node;
            }
            return self;
        }
        else
        {
            return onValue(self);
        }
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    /// <param name="selfPath"></param>
    public static Json EachAll(this Json self,JsonPath selfPath, Func<JsonPath,Json, Json> onValue)
    {
        if (self.IsArray)
        {
            var array = self.AsArray;
            int count = array.Count;
            for (var i = 0; i < count; i++)
            {
                array[i] = EachAll(new(array[i]), selfPath.Next(i), onValue).Node;
            }
            return self;
        }
        else if (self.IsObject)
        {
            var obj = self.AsObject;
            var lst = obj.ToArray();
            foreach (var item in lst)
            {
                obj[item.Key] = EachAll(new(item.Value), selfPath.Next(item.Key), onValue).Node;
            }
            return self;
        }
        else
        {
            return onValue(selfPath, self);
        }
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    /// <returns></returns>
    public static Json EachAll(this Json self, Func<JsonPath, Json, Json> onValue)
    {
        return EachAll(self, [], onValue);
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    /// <returns></returns>
    public static async Task EachAll(this Json self,Func<Json,Task> onValue)
    {
        if (self.IsArray)
        {
            foreach (var item in self)
            {
                await EachAll(item, onValue);
            }
        }
        else if (self.IsObject)
        {
            foreach(var item in self.GetObjectEnumerable())
            {
                await EachAll(item.Value, onValue);
            }
        }
        else
        {
            await onValue(self);
        }
    }

    /// <summary>
    /// 遍历每一个值
    /// </summary>
    /// <param name="self"></param>
    /// <param name="onValue"></param>
    /// <returns></returns>
    public static async Task<Json> EachAll(this Json self,Func<Json,Task<Json>> onValue)
    {
        if (self.IsArray)
        {
            var array = self.AsArray;
            int count = array.Count;
            for (var i = 0; i < count; i++)
            {
                var item = await EachAll(new(array[i]), onValue);
                if (item.Node != array[i])
                {
                    array[i] = item.Node;
                }
            }
            return self;
        }
        else if (self.IsObject)
        {
            var obj = self.AsObject;
            var lst = obj.ToArray();
            foreach(var item in lst)
            {
                var value = await EachAll(new(item.Value), onValue);
                if (value.Node != item.Value)
                {
                    obj[item.Key] = value.Node;
                }
            }
            return self;
        }
        else
        {
            return await onValue(self);
        }
    }

    /// <summary>
    /// Convert to JsonIndex
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<JsonIndex> ToJsonIndex(this IEnumerable<string> self)
    {
        return self.Select(i => new JsonIndex(i));
    }

    /// <summary>
    /// Convert to JsonIndex
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static IEnumerable<JsonIndex> ToJsonIndex(this IEnumerable<int> self)
    {
        return self.Select(i => new JsonIndex(i));
    }
}
