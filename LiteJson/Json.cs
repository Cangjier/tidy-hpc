using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using TidyHPC.Common;

namespace TidyHPC.LiteJson;

/// <summary>
/// Json Processor
/// </summary>
public readonly partial struct Json : IDisposable, IEnumerable<Json>, IEquatable<Json>
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="node"></param>
    public Json(object? node)
    {
        if (node is Json json)
        {
            Node = json.Node;
        }
        else
        {
            Node = node;
        }
    }

    /// <summary>
    /// Json Node
    /// </summary>
    public object? Node { get; }

    /// <summary>
    /// Get the kind of value
    /// </summary>
    /// <returns></returns>
    public JsonValueKind GetValueKind()
    {
        if (Node is null)
        {
            return JsonValueKind.Null;
        }
        else if (Node is JsonNode jsonNode) return jsonNode.GetValueKind();
        else if (Node is string) return JsonValueKind.String;
        else if (Node is DateTime) return JsonValueKind.String;
        else if (Node is char) return JsonValueKind.String;
        else if (Node is Guid) return JsonValueKind.String;
        else if (Node is JsonElement jsonElement) return jsonElement.ValueKind;
        else if (Node is IDictionary) return JsonValueKind.Object;
        else if (Node is IEnumerable) return JsonValueKind.Array;
        else if (Node is Array) return JsonValueKind.Array;
        else if (Node is int || Node is long || Node is short || Node is byte
            || Node is sbyte || Node is uint || Node is ulong || Node is ushort
            || Node is float || Node is double || Node is decimal) return JsonValueKind.Number;
        else if (Node is true) return JsonValueKind.True;
        else if (Node is false) return JsonValueKind.False;


        return JsonValueKind.Object;
    }

    /// <summary>
    /// New Object
    /// </summary>
    /// <returns></returns>
    public static Json NewObject()
    {
        return new(new Dictionary<string, object?>());
    }

    /// <summary>
    /// New Array
    /// </summary>
    /// <returns></returns>
    public static Json NewArray()
    {
        return new(new List<object?>());
    }

    /// <summary>
    /// Process the Json
    /// </summary>
    /// <param name="onProcess"></param>
    public void Process(Action<Json> onProcess)
    {
        onProcess(this);
    }

    /// <summary>
    /// Clone Json
    /// </summary>
    /// <returns></returns>
    public Json Clone()
    {
        if (Node is JsonNode jsonNode)
        {
            return jsonNode.DeepClone();
        }
        else if (Node is List<object?> listObject)
        {
            return new List<object?>(listObject);
        }
        else if (Node is Dictionary<string, object?> dictionary)
        {
            return new Dictionary<string, object?>(dictionary);
        }
        else
        {
            return new(Node);
        }
    }

    /// <summary>
    /// Clear children
    /// </summary>
    public void Clear()
    {
        if (Node is JsonObject jsonObject)
        {
            jsonObject.Clear();
        }
        else if (Node is JsonArray jsonArray)
        {
            jsonArray.Clear();
        }
        else if (Node is IDictionary dictionary)
        {
            dictionary.Clear();
        }
        else if (Node is IList list)
        {
            list.Clear();
        }
        else if (Node is Array)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Null
    /// </summary>
    public readonly static Json Null = new(null);

    /// <summary>
    /// Undefined
    /// </summary>
    public static Json Undefined => "undefined-622102F6-FF98-4CB6-887B-175F4C1024B0";

    /// <summary>
    /// <para>If self is Array, get the count of Array</para>
    /// <para>If self is Object,get the count of Object </para>
    /// <para>If self is String, get the length of String</para>
    /// </summary>
    public readonly int Count
    {
        get
        {
            if (Node == null) return 0;
            if (Node is JsonObject jsonObject) return jsonObject.Count;
            else if (Node is JsonArray jsonArray) return jsonArray.Count;
            else if (Node is IDictionary dictionary) return dictionary.Count;
            else if (Node is IList list) return list.Count;
            else if (Node is Array array) return array.Length;
            else if (Node is string stringValue) return stringValue.Length;
            else return 0;
        }
    }

    private static JavaScriptEncoder JavaScriptEncoder { get; } = JavaScriptEncoder.UnsafeRelaxedJsonEscaping; //JavaScriptEncoder.Create(UnicodeRanges.All);

    private static JsonSerializerOptions JsonSerializerOptionsIndented { get; } = new()
    {
#if NET8_0_OR_GREATER
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
#endif
        Encoder = JavaScriptEncoder,
        WriteIndented = true,
        Converters = { new UnsupportedConverter() }
    };

    private static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
#if NET8_0_OR_GREATER
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
#endif
        Encoder = JavaScriptEncoder,
        WriteIndented = false,
        Converters = { new UnsupportedConverter() }
    };

    private static JsonSerializerOptions JsonDeserializerOptions { get; } = new()
    {
#if NET8_0_OR_GREATER
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
#endif
        Encoder = JavaScriptEncoder,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new ObjectConverter() }
    };

    private readonly static JsonWriterOptions JsonWriterOptions = new()
    {
        Encoder = JavaScriptEncoder
    };

    /// <summary>
    /// To String
    /// </summary>
    /// <returns></returns>
    public readonly override string ToString()
    {
        if (IsUndefined)
        {
            return "undefined";
        }
        else if (IsNull)
        {
            return "null";
        }
        else if (IsString) return AsString;
        else if (IsNumber) return AsNumber.ToString();
        else if (IsBoolean) return AsBoolean ? "true" : "false";
        return JsonSerializer.Serialize(Node, JsonSerializerOptionsIndented);
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <param name="indented"></param>
    /// <returns></returns>
    public readonly string ToString(bool indented)
    {
        if (IsUndefined)
        {
            return "undefined";
        }
        else if (IsNull)
        {
            return "null";
        }
        else if (IsString) return AsString;
        else if (IsNumber) return AsNumber.ToString();
        else if (IsBoolean) return AsBoolean ? "true" : "false";
        return JsonSerializer.Serialize(Node, indented ? JsonSerializerOptionsIndented : JsonSerializerOptions);
    }

    /// <summary>
    /// Parse string to Json
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Json Parse(string value)
    {
        return new(JsonSerializer.Deserialize<object>(value, JsonDeserializerOptions));
    }

    /// <summary>
    /// Try Parse string to Json
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryParse(string value, out Json result)
    {
        try
        {
            result = Parse(value);
            return true;
        }
        catch
        {
            result = Null;
            return false;
        }
    }

    /// <summary>
    /// Parse Stream to Json
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static Json Parse(Stream stream)
    {
        return new(JsonSerializer.Deserialize<object>(stream, JsonDeserializerOptions));
    }

    /// <summary>
    /// Parse Stream to Json
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<Json> ParseAsync(Stream stream)
    {
        return new(await JsonSerializer.DeserializeAsync<object>(stream, JsonDeserializerOptions));
    }

    /// <summary>
    /// Try Parse stream to Json
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static async Task<bool> TryParseAsync(Stream stream, Ref<Json> result)
    {
        try
        {
            result.Value = await ParseAsync(stream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parse bytes to Json
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static Json Parse(byte[] bytes)
    {
        return new(JsonSerializer.Deserialize<object>(bytes, JsonDeserializerOptions));
    }

    /// <summary>
    /// Load Json from file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Json Load(string path)
    {
        using var fileStream = File.OpenRead(path);
        return Parse(fileStream);
    }

    /// <summary>
    /// Repair Json string
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    public static string Repair(string raw)
    {
        raw = JsonRepair.RepairQuote(raw);
        raw = JsonRepair.RepairIndent(raw);
        return raw;
    }
    
    /// <summary>
    /// Load Json from file until timeout
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public static async Task<Json> LoadUntilTimeout(string path)
    {
        int timeout = 1000;
        while (true)
        {
            if (timeout <= 0)
            {
                throw new TimeoutException($"Load {path} timeout");
            }
            if (File.Exists(path) == false)
            {
                await Task.Delay(10);
                timeout -= 10;
                continue;
            }
            try
            {
                return Load(path);
            }
            catch
            {
                await Task.Delay(10);
                timeout -= 10;
                continue;
            }
        }
    }

    /// <summary>
    /// Load Json from file async
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<Json> LoadAsync(string path)
    {
        using var fileStream = File.OpenRead(path);
        return await ParseAsync(fileStream);
    }

    /// <summary>
    /// Try Load Json from file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    public static Json TryLoad(string? path, Func<Json> onDefaultValue)
    {
        if (string.IsNullOrEmpty(path))
        {
            return onDefaultValue();
        }
        else if (!File.Exists(path))
        {
            return onDefaultValue();
        }
        try
        {
            return Load(path);
        }
        catch
        {
            return onDefaultValue();
        }
    }

    /// <summary>
    /// Validate Json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static bool Validate(string json)
    {
        try
        {
            Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static bool Validate(Stream stream)
    {
        try
        {
            Parse(stream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Save Json to file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="indented"></param>
    /// <exception cref="Exception"></exception>
    public void Save(string path, bool indented = true)
    {
        if (Node == null) throw new Exception("Node is null");
        if (File.Exists(path)) File.Delete(path);
        using var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var utfWriter = new Utf8JsonWriter(fileStream, JsonWriterOptions);
        JsonSerializer.Serialize(utfWriter, Node, indented ? JsonSerializerOptionsIndented : JsonSerializerOptions);
        utfWriter.Flush();
    }

    /// <summary>
    /// Save to Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="indented"></param>
    /// <exception cref="Exception"></exception>
    public void Save(Stream stream, bool indented = true)
    {
        if (Node == null) throw new Exception("Node is null");
        using var utfWriter = new Utf8JsonWriter(stream, JsonWriterOptions);
        JsonSerializer.Serialize(utfWriter, Node, indented ? JsonSerializerOptionsIndented : JsonSerializerOptions);
        utfWriter.Flush();
    }

    /// <summary>
    /// Write to Stream
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="Exception"></exception>
    public void WriteTo(Stream stream)
    {
        if (Node == null) throw new Exception("Node is null");
        using var utfWriter = new Utf8JsonWriter(stream, JsonWriterOptions);
        JsonSerializer.Serialize(utfWriter, Node, JsonSerializerOptions);
        utfWriter.Flush();
    }

    /// <summary>
    /// Assert Object
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void AssertObject()
    {
        if (!IsObject)
        {
            throw new Exception($"Not Object, {ToString()}");
        }
    }

    /// <summary>
    /// Assert Object
    /// </summary>
    /// <param name="onObject"></param>
    /// <exception cref="Exception"></exception>
    public void AssertObject(Action<ObjectWrapper> onObject)
    {
        if (!IsObject)
        {
            throw new Exception($"Not Object, {ToString()}");
        }
        onObject(this);
    }

    /// <summary>
    /// Assert Array
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void AssertArray()
    {
        if (!IsArray)
        {
            throw new Exception($"Not Array, {ToString()}");
        }
    }

    /// <summary>
    /// Assert Array
    /// </summary>
    /// <param name="onArray"></param>
    /// <exception cref="Exception"></exception>
    public void AssertArray(Action<ArrayWrapper> onArray)
    {
        if (!IsArray)
        {
            throw new Exception($"Not Array, {ToString()}");
        }
        onArray(this);
    }

    /// <summary>
    /// Release
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        if (IsObject)
        {
            new ObjectWrapper(Node).Dispose();
        }
        else if (IsArray)
        {
            new ArrayWrapper(Node).Dispose();
        }
        else
        {

        }
    }

    #region Enumerable

    /// <summary>
    /// Get the enumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Json> GetEnumerator()
    {
        if (IsArray)
        {
            foreach (var item in AsArray)
            {
                yield return new(item);
            }
        }
    }

    /// <summary>
    /// Get the enumerator
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        if (IsArray)
        {
            foreach (var item in AsArray)
            {
                yield return new Json(item);
            }
        }
        else if (IsObject)
        {
            foreach (var item in AsObject)
            {
                yield return new KeyValuePair<string, Json>(item.Key, new Json(item.Value));
            }
        }
    }

    /// <summary>
    /// Get the enumerable
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<string, Json>> GetObjectEnumerable()
    {
        AssertObject();
        foreach (var item in AsObject)
        {
            yield return new KeyValuePair<string, Json>(item.Key, new Json(item.Value));
        }
    }

    /// <summary>
    /// Get the enumerable
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Json> GetArrayEnumerable() => this;

    /// <summary>
    /// To list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public List<T> ToList<T>(Func<Json, T> selector)
    {
        var result = new List<T>();
        foreach (var item in this)
        {
            result.Add(selector(item));
        }
        return result;
    }

    /// <summary>
    /// To array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public T[] ToArray<T>(Func<Json, T> selector)
    {
        var result = new T[Count];
        int index = 0;
        foreach (var item in this)
        {
            result[index++] = selector(item);
        }
        return result;
    }

    /// <summary>
    /// Foreach
    /// </summary>
    /// <param name="onEach"></param>
    public void ForeachArray(Action<Json> onEach)
    {
        foreach (var item in this)
        {
            onEach(item);
        }
    }

    /// <summary>
    /// Foreach index value
    /// </summary>
    /// <param name="onIndex"></param>
    public void ForeachArray(Action<int, Json> onIndex)
    {
        int index = 0;
        foreach (var item in this)
        {
            onIndex(index++, item);
        }
    }

    /// <summary>
    /// Select
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public IEnumerable<T> SelectArray<T>(Func<Json, T> selector)
    {
        foreach (var item in this)
        {
            yield return selector(item);
        }
    }

    /// <summary>
    /// Where
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IEnumerable<Json> WhereArray(Func<Json, bool> predicate)
    {
        foreach (var item in this)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Foreach value
    /// </summary>
    /// <param name="onValue"></param>
    public void ForeachObject(Action<Json> onValue)
    {
        foreach (var item in GetObjectEnumerable())
        {
            onValue(item.Value);
        }
    }

    /// <summary>
    /// Foreach KeyValuePair
    /// </summary>
    /// <param name="onKeyValuePair"></param>
    public void ForeachObject(Action<string, Json> onKeyValuePair)
    {
        foreach (var item in GetObjectEnumerable())
        {
            onKeyValuePair(item.Key, item.Value);
        }
    }

    #endregion

    /// <inheritdoc/>
    public bool Equals(Json other)
    {
        if (Node == null && other.Node == null)
        {
            return true;
        }
        if (Node == null || other.Node == null)
        {
            return false;
        }
        var valueKind = GetValueKind();
        if (valueKind == other.GetValueKind())
        {
            if (Node is JsonNode jsonNode && other.Node is JsonNode otherJsonNode)
            {
#if NET6_0
                return JsonExtensions.DeepEquals(jsonNode, otherJsonNode);
#else
                return JsonNode.DeepEquals(jsonNode, otherJsonNode);
#endif
            }
            else if (valueKind == JsonValueKind.String)
            {
                return AsString == other.AsString;
            }
            else if (valueKind == JsonValueKind.Number)
            {
                return AsNumber == other.AsNumber;
            }
            else
            {
                return Node.Equals(other.Node);
            }
        }
        else if (Node.GetType().IsEnum || other.Node.GetType().IsEnum)
        {
            var nodeType = Node.GetType();
            var otherType = other.Node.GetType();
            if (nodeType.IsEnum && otherType.IsEnum)
            {
                return Node.Equals(other.Node);
            }
            else if (nodeType.IsEnum && otherType == typeof(int))
            {
                var rightValue = Enum.ToObject(nodeType, (int)other.Node);
                var rightString = rightValue?.ToString();
                return Node.ToString() == rightString;
            }
            else if (nodeType.IsEnum && otherType == typeof(string))
            {
                return Enum.GetName(nodeType, Node) == other.Node.ToString();
            }
            else if (otherType.IsEnum && nodeType == typeof(int))
            {
                return other.Node.ToString() == Enum.GetValues(otherType).GetValue((int)Node)?.ToString();
            }
            else if (otherType.IsEnum && nodeType == typeof(string))
            {
                return Enum.GetName(otherType, other.Node) == Node.ToString();
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        if (other is Json otherJson)
        {
            return Equals(otherJson);
        }
        else return Equals(new Json(other));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Node?.GetHashCode() ?? 0;
    }

}


