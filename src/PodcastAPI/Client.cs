using System.Reflection;
using PodcastAPI.Exceptions;

namespace PodcastAPI;

/// <summary>A reusable client. Without an API key, requests go to the public mock server.</summary>
public sealed partial class Client : IDisposable
{
    public readonly string BASE_URL_TEST = "https://listen-api-test.listennotes.com/api/v2";
    public readonly string BASE_URL_PROD = "https://listen-api.listennotes.com/api/v2";
    public readonly string userAgent;
    public Uri BaseUrl { get; }
    public TimeSpan Timeout { get; }

    private readonly HttpClient httpClient;
    private readonly bool ownsHttpClient;
    private readonly string? apiKey;
    private bool disposed;

    /// <param name="apiKey">A Listen API key; null or whitespace selects the mock server.</param>
    /// <param name="httpClient">Optional caller-owned transport. It must disable redirects and retries.</param>
    /// <param name="baseUrl">Optional API base URL. The API key will be sent to this destination.</param>
    /// <param name="timeout">Per-request timeout, defaulting to 30 seconds.</param>
    public Client(string? apiKey = null, HttpClient? httpClient = null, Uri? baseUrl = null,
        TimeSpan? timeout = null)
    {
        this.apiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
        var destination = baseUrl ?? new Uri(this.apiKey is null ? BASE_URL_TEST : BASE_URL_PROD);
        if (!destination.IsAbsoluteUri || destination.Scheme is not ("http" or "https") ||
            destination.Query.Length != 0 || destination.Fragment.Length != 0 || destination.UserInfo.Length != 0)
            throw new ArgumentException("Use an absolute HTTP(S) base URL without credentials, query, or fragment.", nameof(baseUrl));
        BaseUrl = new Uri(destination.AbsoluteUri.TrimEnd('/') + "/");
        Timeout = timeout ?? TimeSpan.FromSeconds(30);
        if (Timeout <= TimeSpan.Zero || Timeout.TotalMilliseconds > uint.MaxValue - 1)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Use a positive, finite timeout.");
        var version = typeof(Client).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion.Split('+')[0];
        userAgent = $"podcast-api-dotnet {version}";
        ownsHttpClient = httpClient is null;
        this.httpClient = httpClient ?? new HttpClient(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        }) { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
    }

    private async Task<ApiResponse> Request(string path, HttpMethod method, string[] pathNames,
        string[] queryNames, IDictionary<string, string>? parameters, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        // Snapshot the parameters: path extraction must never modify the caller's dictionary.
        var remaining = parameters is null
            ? new Dictionary<string, string>() : new Dictionary<string, string>(parameters, StringComparer.Ordinal);
        foreach (var name in pathNames)
        {
            if (!remaining.Remove(name, out var value) || string.IsNullOrWhiteSpace(value) || value is "." or "..")
                throw new ArgumentException($"Missing or invalid path parameter: {name}", nameof(parameters));
            path = path.Replace("{" + name + "}", Uri.EscapeDataString(value), StringComparison.Ordinal);
        }
        var query = new List<KeyValuePair<string, string>>();
        var body = new List<KeyValuePair<string, string>>();
        var hasBody = method == HttpMethod.Post || method == HttpMethod.Put;
        foreach (var parameter in remaining)
        {
            if (parameter.Value is null)
                continue;
            if (!hasBody || queryNames.Contains(parameter.Key, StringComparer.Ordinal))
                query.Add(parameter);
            else
                body.Add(parameter);
        }
        var uri = new Uri(BaseUrl, path.TrimStart('/'));
        if (query.Count > 0)
        {
            using var encodedQuery = new FormUrlEncodedContent(query);
            uri = new Uri(uri.AbsoluteUri + "?" + await encodedQuery.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("User-Agent", userAgent);
        request.Headers.Add("Accept", "application/json");
        if (apiKey is not null)
            request.Headers.Add("X-ListenAPI-Key", apiKey);
        if (hasBody)
            request.Content = new FormUrlEncodedContent(body);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(Timeout);
        try
        {
            // No SDK retries: replaying a write after an ambiguous failure could duplicate data.
            using var response = await httpClient.SendAsync(request, deadline.Token).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync(deadline.Token).ConfigureAwait(false);
            var result = new ApiResponse(json, response);
            if (!response.IsSuccessStatusCode)
                throw HttpError(result);
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiConnectionException("The Listen API request timed out.");
        }
        catch (HttpRequestException)
        {
            throw new ApiConnectionException("Failed to connect to Listen API servers.");
        }
    }

    private static ListenApiException HttpError(ApiResponse response)
    {
        var status = (int)response.StatusCode;
        ListenApiException error = status switch
        {
            400 => new InvalidRequestException("Invalid Listen API request (HTTP 400)."),
            401 => new AuthenticationException("Invalid API key or suspended account (HTTP 401)."),
            403 => new PermissionDeniedException("The API account cannot access this resource (HTTP 403)."),
            404 => new NotFoundException("The requested resource was not found (HTTP 404)."),
            429 => new RateLimitException("Listen API quota or rate limit exceeded (HTTP 429)."),
            _ => new ListenApiException($"Listen API returned HTTP {status}."),
        };
        error.Response = response;
        return error;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (ownsHttpClient) httpClient.Dispose();
    }
}
