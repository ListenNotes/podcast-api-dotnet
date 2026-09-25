using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PodcastAPI.Exceptions;

[assembly: DoNotParallelize]

namespace PodcastAPI.Tests;

[TestClass]
public class ClientTests
{
    [TestMethod]
    public void CiUsesTheExpectedRuntime()
    {
        var expected = Environment.GetEnvironmentVariable("LISTEN_API_TEST_RUNTIME");
        if (expected is not null) Assert.AreEqual(expected, Environment.Version.Major.ToString());
    }

    private static readonly JsonElement Contract = JsonDocument.Parse(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "api-contract.json")))
        .RootElement;
    internal static readonly JsonElement[] Operations = Contract.GetProperty("operations")
        .EnumerateArray().Select(op => op.Clone()).ToArray();

    public static IEnumerable<object[]> OperationCases => Operations.Select(op => new object[] { op.GetProperty("operationId").GetString()! });

    internal static Dictionary<string, string> Examples(JsonElement op) => op.GetProperty("example_params")
        .EnumerateObject().ToDictionary(p => p.Name, p => p.Value.ValueKind == JsonValueKind.String
            ? p.Value.GetString()! : p.Value.GetRawText());

    internal sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public bool Disposed { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            return send(request, token);
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }

    internal static HttpResponseMessage Response(int status = 200, string json = "{\"ok\":true}") =>
        new((HttpStatusCode)status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    internal static Dictionary<string, string> Decode(string? content) => string.IsNullOrEmpty(content)
        ? new() : content.TrimStart('?').Split('&').Select(p => p.Split('=', 2)).ToDictionary(
            p => WebUtility.UrlDecode(p[0]), p => p.Length == 2 ? WebUtility.UrlDecode(p[1]) : "");

    [TestMethod]
    [DynamicData(nameof(OperationCases))]
    public async Task EveryGeneratedMethodMatchesContract(string operation)
    {
        var op = Operations.Single(op => op.GetProperty("operationId").GetString() == operation);
        var parameters = Examples(op);
        var before = new Dictionary<string, string>(parameters);
        using var handler = new Handler(async (request, token) =>
        {
            var expectedPath = op.GetProperty("path").GetString()!;
            var body = new Dictionary<string, string>();
            var query = new Dictionary<string, string>();
            foreach (var param in op.GetProperty("parameters").EnumerateArray())
            {
                var name = param.GetProperty("name").GetString()!;
                if (!parameters.TryGetValue(name, out var value)) continue;
                switch (param.GetProperty("in").GetString())
                {
                    case "path": expectedPath = expectedPath.Replace("{" + name + "}", Uri.EscapeDataString(value)); break;
                    case "query": query[name] = value; break;
                    case "body": body[name] = value; break;
                }
            }
            Assert.AreEqual(op.GetProperty("method").GetString(), request.Method.Method);
            Assert.AreEqual("/api/v2" + expectedPath, request.RequestUri!.AbsolutePath);
            CollectionAssert.AreEquivalent(query.ToArray(), Decode(request.RequestUri.Query).ToArray());
            CollectionAssert.AreEquivalent(body.ToArray(), Decode(request.Content is null ? null : await request.Content.ReadAsStringAsync(token)).ToArray());
            Assert.AreEqual("fixture-key", request.Headers.GetValues("X-ListenAPI-Key").Single());
            Assert.AreEqual("podcast-api-dotnet " + Contract.GetProperty("version").GetString(), request.Headers.UserAgent.ToString());
            Assert.AreEqual("application/json", request.Headers.Accept.Single().MediaType);
            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                Assert.AreEqual("application/x-www-form-urlencoded", request.Content!.Headers.ContentType!.MediaType);
            else Assert.IsNull(request.Content);
            return Response(request.Method == HttpMethod.Post ? 201 : 200);
        });
        using var http = new HttpClient(handler);
        using var client = new Client("fixture-key", http);
        var response = await MethodDispatch.Call(operation, client, parameters);
        Assert.IsTrue(response.ToJSON<dynamic>()!.ok.Value);
        Assert.AreEqual(op.GetProperty("method").GetString() == "POST" ? 201 : 200, (int)response.StatusCode);
        CollectionAssert.AreEquivalent(before.ToArray(), parameters.ToArray());
        Assert.AreEqual(1, handler.Calls);
    }

    [TestMethod]
    public async Task DeletePlaylistEncodesItsIdentifierWithoutQueryOrBody()
    {
        const string id = "a/b ?#%é";
        var parameters = new Dictionary<string, string> { ["id"] = id };
        using var handler = new Handler((request, _) =>
        {
            Assert.AreEqual(HttpMethod.Delete, request.Method);
            Assert.AreEqual("/api/v2/playlists/a%2Fb%20%3F%23%25%C3%A9", request.RequestUri!.AbsolutePath);
            Assert.AreEqual("", request.RequestUri.Query);
            Assert.IsNull(request.Content);
            var response = Response(200, "{\"id\":\"a/b ?#%é\",\"deleted\":true}");
            response.Headers.Add("X-ListenAPI-Usage", "12");
            return Task.FromResult(response);
        });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        var response = await client.DeletePlaylist(parameters);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(id, (string)response.ToJSON<dynamic>()!.id);
        Assert.AreEqual(true, (bool)response.ToJSON<dynamic>()!.deleted);
        Assert.AreEqual(12, response.GetUsage());
        Assert.AreEqual(id, parameters["id"]);
        Assert.AreEqual(1, parameters.Count);
        Assert.AreEqual(1, handler.Calls);
    }

    [TestMethod]
    public async Task NestedIdentifiersQueryAndEmptyBodyValuesAreEncoded()
    {
        using var handler = new Handler(async (request, token) =>
        {
            switch (request.Method.Method)
            {
                case "PUT":
                    Assert.AreEqual("/api/v2/playlists/a%2Fb%20%3F%23/items/x%2By%25", request.RequestUri!.AbsolutePath);
                    CollectionAssert.AreEquivalent(new[] { new KeyValuePair<string, string>("notes", "") }, Decode(await request.Content!.ReadAsStringAsync(token)).ToArray());
                    Assert.AreEqual("", request.RequestUri.Query);
                    break;
                case "POST":
                    Assert.AreEqual("name=Caf%C3%A9+%26+tea&description=", await request.Content!.ReadAsStringAsync(token));
                    break;
                case "DELETE":
                    Assert.AreEqual("reason=space+%26+%2B+%3F+%23+%E9%9B%AA", request.RequestUri!.Query.TrimStart('?'));
                    break;
                case "GET":
                    Assert.AreEqual("?q=a%2Bb+%26+caf%C3%A9&offset=0&safe_mode=0", request.RequestUri!.Query);
                    break;
            }
            return Response();
        });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        var parameters = new Dictionary<string, string> { ["id"] = "a/b ?#", ["item_id"] = "x+y%", ["notes"] = "" };
        await client.UpdatePlaylistItemNotes(parameters);
        Assert.AreEqual(3, parameters.Count);
        await client.CreatePlaylist(new Dictionary<string, string> { ["name"] = "Café & tea", ["description"] = "" });
        await client.DeletePodcast(new Dictionary<string, string> { ["id"] = "podcast", ["reason"] = "space & + ? # 雪" });
        await client.Search(new Dictionary<string, string> { ["q"] = "a+b & café", ["offset"] = "0", ["safe_mode"] = "0" });
    }

    [TestMethod]
    public async Task RepeatedWritesAreNotRetriedAndOmittedNotesStayOmitted()
    {
        using var handler = new Handler(async (request, token) =>
        {
            Assert.AreEqual("episode_id=episode", await request.Content!.ReadAsStringAsync(token));
            return Response(200, "{\"id\":23}");
        });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        var p = new Dictionary<string, string> { ["id"] = "playlist", ["episode_id"] = "episode" };
        Assert.AreEqual(23, (int)(await client.AddPlaylistItem(p)).ToJSON<dynamic>()!.id);
        Assert.AreEqual(23, (int)(await client.AddPlaylistItem(p)).ToJSON<dynamic>()!.id);
        Assert.AreEqual(2, handler.Calls);
        Assert.AreEqual("playlist", p["id"]);
    }

    [TestMethod]
    [DataRow(400, typeof(InvalidRequestException))]
    [DataRow(401, typeof(AuthenticationException))]
    [DataRow(403, typeof(PermissionDeniedException))]
    [DataRow(404, typeof(NotFoundException))]
    [DataRow(429, typeof(RateLimitException))]
    [DataRow(500, typeof(ListenApiException))]
    [DataRow(503, typeof(ListenApiException))]
    [DataRow(302, typeof(ListenApiException))]
    [DataRow(307, typeof(ListenApiException))]
    [DataRow(308, typeof(ListenApiException))]
    [DataRow(422, typeof(ListenApiException))]
    public async Task HttpErrorsRetainResponseWithoutRetries(int status, Type type)
    {
        using var handler = new Handler((_, _) =>
        {
            var response = Response(status, "{\"error\":\"Exact reason\"}");
            response.Headers.Add("X-ListenAPI-Usage", "123");
            return Task.FromResult(response);
        });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        foreach (var operation in new[] { "createPlaylist", "deletePlaylist" })
        {
            var parameters = operation == "createPlaylist"
                ? new Dictionary<string, string> { ["name"] = "test" }
                : new Dictionary<string, string> { ["id"] = "playlist" };
            var error = await Assert.ThrowsAsync<ListenApiException>(() => MethodDispatch.Call(operation, client, parameters));
            Assert.AreEqual(type, error.GetType());
            Assert.AreEqual(status, (int)error.Response!.StatusCode);
            Assert.AreEqual("Exact reason", (string)error.Response.ToJSON<dynamic>()!.error);
            Assert.AreEqual(123, error.Response.GetUsage());
        }
        Assert.AreEqual(2, handler.Calls);
    }

    [TestMethod]
    public async Task HeadersAreCaseInsensitiveAndResponsesAreBuffered()
    {
        using var handler = new Handler((_, _) =>
        {
            var response = Response();
            response.Headers.Add("X-ListenAPI-FreeQuota", "25000");
            response.Headers.Add("X-ListenAPI-USAGE", "19231");
            response.Headers.Add("x-listenAPI-nextBillingDate", "2026-10-01T00:00:00+00:00");
            response.Headers.Add("X-ListenAPI-Latency-Seconds", "0.056");
            return Task.FromResult(response);
        });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        var result = await client.FetchPodcastLanguages();
        Assert.AreEqual(25000, result.GetFreeQuota());
        Assert.AreEqual(19231, result.GetUsage());
        Assert.AreEqual("2026-10-01T00:00:00+00:00", result.GetNextBillingDate());
        Assert.AreEqual("0.056", result.Headers["x-listenapi-latency-seconds"]);
        Assert.AreEqual("{\"ok\":true}", result.ToString());
        using var raw = Response();
        var empty = new ApiResponse("{}", raw);
        Assert.ThrowsExactly<InvalidOperationException>(() => empty.GetFreeQuota());
        Assert.ThrowsExactly<InvalidOperationException>(() => empty.GetUsage());
        Assert.IsNull(empty.GetNextBillingDate());
        raw.Headers.Add("X-ListenAPI-Usage", "invalid");
        Assert.ThrowsExactly<InvalidOperationException>(() => new ApiResponse("{}", raw).GetUsage());
    }

    [TestMethod]
    public async Task ClientsKeepTheirKeysDestinationsAndBorrowedTransportIndependent()
    {
        var seen = new List<string>();
        using var handler = new Handler((request, _) =>
        {
            seen.Add(request.RequestUri!.Host + ":" + (request.Headers.TryGetValues("X-ListenAPI-Key", out var values) ? values.Single() : "none"));
            return Task.FromResult(Response());
        });
        using var http = new HttpClient(handler);
        using var first = new Client("first-key", http);
        using var second = new Client("second-key", http, new Uri("https://second.example/api/v2/"));
        using var mock = new Client("   ", http);
        await first.FetchPodcastLanguages();
        await second.FetchPodcastLanguages();
        first.Dispose();
        Assert.IsFalse(handler.Disposed);
        await mock.FetchPodcastLanguages();
        await second.FetchPodcastLanguages();
        CollectionAssert.AreEqual(new[] { "listen-api.listennotes.com:first-key", "second.example:second-key", "listen-api-test.listennotes.com:none", "second.example:second-key" }, seen);
        Assert.IsFalse(http.DefaultRequestHeaders.Contains("X-ListenAPI-Key"));
        Assert.IsFalse(http.DefaultRequestHeaders.Contains("User-Agent"));
        Assert.AreEqual(TimeSpan.FromSeconds(30), second.Timeout);
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => first.FetchPodcastLanguages());
    }

    [TestMethod]
    public async Task PathValidationHappensBeforeSending()
    {
        using var handler = new Handler((_, _) => Task.FromResult(Response()));
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http);
        foreach (var value in new[] { "", " ", ".", ".." })
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.FetchPlaylistById(new Dictionary<string, string> { ["id"] = value }));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.DeletePlaylist(new Dictionary<string, string> { ["id"] = value }));
        }
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.DeletePlaylist());
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.DeletePlaylist(new Dictionary<string, string>()));
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => client.DeletePlaylistItem(new Dictionary<string, string> { ["id"] = "playlist" }));
        Assert.AreEqual(0, handler.Calls);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Client(timeout: TimeSpan.Zero));
        foreach (var url in new[] { "file:///tmp/test", "https://example.test/?key=value", "https://example.test/#fragment", "https://user:password@example.test/" })
            Assert.ThrowsExactly<ArgumentException>(() => new Client(baseUrl: new Uri(url)));
    }

    [TestMethod]
    [DataRow("getLanguages")]
    [DataRow("deletePlaylist")]
    public async Task CancellationTimeoutAndConnectionFailureAreDistinct(string operation)
    {
        var parameters = operation == "deletePlaylist"
            ? new Dictionary<string, string> { ["id"] = "playlist" } : new Dictionary<string, string>();
        using var handler = new Handler(async (_, token) => { await Task.Delay(System.Threading.Timeout.Infinite, token); return Response(); });
        using var http = new HttpClient(handler);
        using var client = new Client(httpClient: http, timeout: TimeSpan.FromMilliseconds(50));
        var timeout = await Assert.ThrowsExactlyAsync<ApiConnectionException>(() => MethodDispatch.Call(operation, client, parameters));
        Assert.IsNull(timeout.Response);
        Assert.AreEqual(1, handler.Calls);
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => MethodDispatch.Call(operation, client, parameters, cancel.Token));
        Assert.AreEqual(1, handler.Calls);
        using var inFlight = new CancellationTokenSource(TimeSpan.FromMilliseconds(25));
        using var slower = new Client(httpClient: http, timeout: TimeSpan.FromSeconds(10));
        await Assert.ThrowsAsync<OperationCanceledException>(() => MethodDispatch.Call(operation, slower, parameters, inFlight.Token));
        Assert.AreEqual(2, handler.Calls);
        using var failed = new Handler((_, _) => throw new HttpRequestException("must not leak a secret request URI"));
        using var failedHttp = new HttpClient(failed);
        using var disconnected = new Client(httpClient: failedHttp);
        var error = await Assert.ThrowsExactlyAsync<ApiConnectionException>(() => MethodDispatch.Call(operation, disconnected, parameters));
        Assert.IsFalse(error.ToString().Contains("secret request URI", StringComparison.Ordinal));
        Assert.AreEqual(1, failed.Calls);
    }

    [TestMethod]
    [DataRow("getLanguages", 302)]
    [DataRow("deletePlaylist", 302)]
    [DataRow("deletePlaylist", 307)]
    [DataRow("deletePlaylist", 308)]
    public async Task DefaultTransportDoesNotFollowRedirects(string operation, int status)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var server = Task.Run(async () =>
        {
            using var connection = await listener.AcceptTcpClientAsync(deadline.Token);
            await using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, leaveOpen: true);
            var expectedRequest = operation == "deletePlaylist"
                ? "DELETE /api/v2/playlists/playlist HTTP/1.1" : "GET /api/v2/languages HTTP/1.1";
            Assert.AreEqual(expectedRequest, await reader.ReadLineAsync(deadline.Token));
            while (!string.IsNullOrEmpty(await reader.ReadLineAsync(deadline.Token))) { }
            var response = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Redirect\r\nLocation: http://127.0.0.1:{port}/redirected\r\nContent-Length: 2\r\nConnection: close\r\n\r\n{{}}");
            await stream.WriteAsync(response, deadline.Token);
        }, deadline.Token);
        using var client = new Client("fixture-key", baseUrl: new Uri($"http://127.0.0.1:{port}/api/v2"));
        var parameters = operation == "deletePlaylist"
            ? new Dictionary<string, string> { ["id"] = "playlist" } : new Dictionary<string, string>();
        var error = await Assert.ThrowsExactlyAsync<ListenApiException>(() => MethodDispatch.Call(operation, client, parameters, deadline.Token));
        Assert.AreEqual(status, (int)error.Response!.StatusCode);
        await server;
        Assert.IsFalse(listener.Pending());
    }
}
