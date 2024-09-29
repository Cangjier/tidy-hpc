using System.Text.Json.Nodes;

namespace TidyHPC.LiteJson;

/// <summary>
/// Json Extensions
/// </summary>
public static class JsonExtensions
{
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
