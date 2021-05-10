using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodcastAPI
{
    public sealed class Client
    {
        private readonly string baseUrlTest = "https://listen-api-test.listennotes.com/api/v2";
        private readonly string baseUrlProd = "https://listen-api.listennotes.com/api/v2";
        private readonly string userAgent;
        private readonly IRestClient restClient;

        public Client(string apiKey = null)
        {
            var version = GetType().Assembly.GetName().Version.ToString();

            userAgent = $"podcasts-api-dotnet {version}";

            var prodMode = !string.IsNullOrWhiteSpace(apiKey);

            restClient = new RestClient(prodMode ? baseUrlProd : baseUrlTest);

            if (prodMode)
            {
                restClient.AddDefaultHeader("X-ListenAPI-Key", apiKey);
            }

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        public async Task<ApiResponse> Search(IDictionary<string, string> parameters)
        {
            var request = new RestRequest("search", Method.GET);

            foreach (var parameter in parameters)
            {
                request.AddQueryParameter(parameter.Key, parameter.Value);
            }

            var response = await restClient.ExecuteAsync(request);

            var result = new ApiResponse(response.Content, response);

            return result;
        }
    }
}
