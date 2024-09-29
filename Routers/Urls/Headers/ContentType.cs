using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace TidyHPC.Routers.Urls.Headers;

/// <summary>
/// Content-Type
/// </summary>
public class ContentType
{
    private static Regex BoundaryRegex = new(@"boundary=(?<boundary>.+)", RegexOptions.Compiled);

    private static Regex CharSetRegex = new(@"charset=(?<charset>.+)", RegexOptions.Compiled);

    private static Regex MediaTypeRegex = new(@"(?<mediaType>.+?)(;|$)", RegexOptions.Compiled);

    private static Regex NameRegex = new(@"name=(?<name>.+)", RegexOptions.Compiled);

    private static Regex ParameterRegex = new(@"(?<key>.+?)=(?<value>.+?)(;|$)", RegexOptions.Compiled);

    /// <summary>
    /// Boundary
    /// </summary>
    public string? Boundary { get; set; } = null;

    /// <summary>
    /// CharSet
    /// </summary>
    public string? CharSet { get; set; } = null;

    /// <summary>
    /// MediaType
    /// </summary>
    public string? MediaType { get; set; } = null;

    /// <summary>
    /// IsApplicationJson
    /// </summary>
    public bool IsApplicationJson => MediaType == "application/json";

    /// <summary>
    /// Is ApplicationFormUrlencoded
    /// </summary>
    public bool IsApplicationFormUrlencoded => MediaType == "application/x-www-form-urlencoded";

    /// <summary>
    /// Is MultipartFormData
    /// </summary>
    public bool IsMultipartFormData => MediaType == "multipart/form-data";

    /// <summary>
    /// Is TextPlain
    /// </summary>
    public bool IsTextPlain => MediaType == "text/plain";

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }=string.Empty;

    /// <summary>
    /// Parameters
    /// </summary>
    public StringDictionary Parameters { get; } = new();

    /// <summary>
    /// Content-Type
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static ContentType Parse(string contentType)
    {
        var result = new ContentType();
        var match = MediaTypeRegex.Match(contentType);
        if (match.Success)
        {
            result.MediaType = match.Groups["mediaType"].Value;
        }
        match = BoundaryRegex.Match(contentType);
        if (match.Success)
        {
            result.Boundary = match.Groups["boundary"].Value;
        }
        match = CharSetRegex.Match(contentType);
        if (match.Success)
        {
            result.CharSet = match.Groups["charset"].Value;
        }
        match = NameRegex.Match(contentType);
        if (match.Success)
        {
            result.Name = match.Groups["name"].Value;
        }
        match = ParameterRegex.Match(contentType);
        while (match.Success)
        {
            result.Parameters.Add(match.Groups["key"].Value, match.Groups["value"].Value);
            match = match.NextMatch();
        }
        return result;
    }

    /// <summary>
    /// Convert to string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (MediaType == null)
        {
            throw new("MediaType is null");
        }
        var result = MediaType;
        if (CharSet != null)
        {
            result += $"; charset={CharSet}";
        }
        if (Boundary != null)
        {
            result += $"; boundary={Boundary}";
        }
        if (Name != string.Empty)
        {
            result += $"; name={Name}";
        }
        foreach (string key in Parameters.Keys)
        {
            result += $"; {key}={Parameters[key]}";
        }
        return result;
    }
}
