using PodcastAPI;
using PodcastAPI.Exceptions;

using var client = new Client(Environment.GetEnvironmentVariable("LISTEN_API_KEY"));
try
{
    var response = await client.Search(new Dictionary<string, string> { ["q"] = "startup", ["type"] = "episode" });
    Console.WriteLine(response.ToJSON<dynamic>());
    Console.WriteLine($"HTTP {(int)response.StatusCode}");
    foreach (var header in response.Headers)
        Console.WriteLine($"{header.Key}: {header.Value}");
}
catch (ListenApiException error)
{
    Console.Error.WriteLine(error.Message);
    Console.Error.WriteLine(error.Response?.ToString());
    Environment.ExitCode = 1;
}
