using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace PodcastAPI;

/// <summary>A buffered response whose JSON and headers remain available after the request completes.</summary>
public sealed class ApiResponse
{
    private readonly string jsonString;
    public HttpStatusCode StatusCode { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }

    public ApiResponse(string jsonString, HttpResponseMessage response)
    {
        this.jsonString = jsonString;
        StatusCode = response.StatusCode;
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers.Concat(response.Content.Headers))
            headers[header.Key] = string.Join(", ", header.Value);
        Headers = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(headers);
    }

    public T? ToJSON<T>() => JsonConvert.DeserializeObject<T>(jsonString);
    public override string ToString() => jsonString;
    public int GetFreeQuota() => GetIntegerHeader("X-ListenAPI-FreeQuota");
    public int GetUsage() => GetIntegerHeader("X-ListenAPI-Usage");
    public string? GetNextBillingDate() => Headers.GetValueOrDefault("X-ListenAPI-NextBillingDate");

    private int GetIntegerHeader(string name)
    {
        if (Headers.TryGetValue(name, out var value) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;
        throw new InvalidOperationException($"Missing or invalid response header: {name}");
    }
}
