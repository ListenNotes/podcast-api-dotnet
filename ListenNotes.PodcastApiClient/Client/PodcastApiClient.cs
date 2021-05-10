using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;

namespace ListenNotes.PodcastApiClient
{
    /// <summary>
    /// Interface for the Api Client for dependency injection 
    /// </summary>
    public interface IPodcastApiClient
    {
        Task<SearchResponse> Search(SearchOptions searchOptions);
    }

    /// <summary>
    /// Use this class for accessing the ListenNotes API
    /// </summary>
    public class PodcastApiClient : IPodcastApiClient
    {
        private readonly IRestClient restClient;
        private readonly string baseUrl;

        /// <summary>
        /// Client Constructor
        /// </summary>
        /// <param name="apiKey">Your ListenNotes API Key.</param>
        /// <param name="testMode">Test Mode determines which base url to use.</param>
        /// <param name="userAgent">If null will use default value. Override if you need to.</param>
        public PodcastApiClient(string apiKey, bool testMode = false, string userAgent = null)
        {
            // as the version changes, make sure it's updated in the requests
            var version = GetType().Assembly.GetName().Version.ToString();

            // default user agent
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                userAgent = $"podcasts-api-dotnet {version}";
            }

            // check for test mode base url
            baseUrl = testMode ? "https://listen-api-test.listennotes.com/api/v2" : "https://listen-api.listennotes.com/api/v2";

            restClient = new RestClient(baseUrl);

            // don't need an api key for test mode. use it for production
            if (!string.IsNullOrWhiteSpace(apiKey) && !testMode)
            {
                restClient.AddDefaultHeader("X-ListenAPI-Key", apiKey);
            }

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        /// <summary>
        /// /search endpoint for the ListenNotes API
        /// </summary>
        /// <param name="searchOptions">The various parameters. Refer to the documentation for required / optional</param>
        /// <returns></returns>
        public async Task<SearchResponse> Search(SearchOptions searchOptions)
        {
            var request = new RestRequest("search", Method.GET);

            request.AddJsonBody(searchOptions);

            var response = await restClient.ExecuteAsync<string>(request);

            var result = JsonConvert.DeserializeObject<SearchResponse>(response.Data);

            return result;
        }
    }
}
