using System.Text.Json;

namespace TidyHPC.Extensions;

/// <summary>
/// JsonDocument Extensions
/// </summary>
public static class JsonDocumentExtensions
{
    /// <summary>
    /// Convert JsonDocument to string
    /// </summary>
    /// <param name="jsonDocument"></param>
    /// <param name="indented"></param>
    /// <returns></returns>
    public static string ToString(this JsonDocument jsonDocument,bool indented)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream,new JsonWriterOptions()
        {
            Indented = indented
        });
        jsonDocument.WriteTo(writer);
        writer.Flush();
        return Util.UTF8.GetString(stream.ToArray());
    }
}
