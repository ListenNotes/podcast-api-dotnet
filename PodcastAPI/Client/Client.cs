using PodcastAPI.Client;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ListenNotes.PodcastApiClient
{
    public sealed class Client
    {
        private readonly string baseUrlTest = "https://listen-api-test.listennotes.com/api/v2";
        private readonly string baseUrlProd = "https://listen-api.listennotes.com/api/v2";
        private readonly string userAgent;
        private readonly IRestClient restClient;

        public Client()
        {
            var version = GetType().Assembly.GetName().Version.ToString();

            userAgent = $"podcasts-api-dotnet {version}";

            restClient = new RestClient(baseUrlTest);

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        public Client(string apiKey)
        {
            var version = GetType().Assembly.GetName().Version.ToString();

            this.userAgent = $"podcasts-api-dotnet {version}";

            this.restClient = new RestClient(baseUrlProd);

            restClient.AddDefaultHeader("X-ListenAPI-Key", apiKey);

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        public async Task<ApiResponse> Search(IDictionary<string, string> parameters)
        {
            var request = new RestRequest("search", Method.GET);

            request.AddJsonBody(parameters);

            var response = await restClient.ExecuteAsync(request);

            var result = new ApiResponse(response.Content, response);

            return result;
        }
    }
}
