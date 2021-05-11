using PodcastAPI.Exceptions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PodcastAPI
{
    public sealed class Client
    {
        public readonly string BASE_URL_TEST = "https://listen-api-test.listennotes.com/api/v2";
        public readonly string BASE_URL_PROD = "https://listen-api.listennotes.com/api/v2";
        public readonly IRestClient restClient;
        private readonly string userAgent;

        public Client(string apiKey = null)
        {
            var version = GetType().Assembly.GetName().Version.ToString();

            userAgent = $"podcasts-api-dotnet {version}";

            var prodMode = !string.IsNullOrWhiteSpace(apiKey);

            restClient = new RestClient(prodMode ? BASE_URL_PROD : BASE_URL_TEST);

            if (prodMode)
            {
                restClient.AddDefaultHeader("X-ListenAPI-Key", apiKey);
            }

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        public async Task<ApiResponse> Search(IDictionary<string, string> parameters)
        {
            var url = "search";
            return await Get(url, parameters);
        }

        private async Task<ApiResponse> Get(string url, IDictionary<string, string> queryParameters)
        {
            try
            {
                var request = new RestRequest(url, Method.GET);

                foreach (var parameter in queryParameters)
                {
                    request.AddQueryParameter(parameter.Key, parameter.Value);
                }

                var response = await restClient.ExecuteAsync(request);

                var result = new ApiResponse(response.Content, response);

                return result;
            }
            catch (Exception ex)
            {
                throw new ListenApiException(ex.Message, ex);
            }
        }

        private async Task<ApiResponse> Post(string url, IDictionary<string, string> bodyParameters)
        {
            try
            {
                var request = new RestRequest(url, Method.POST);

                request.AddJsonBody(bodyParameters);

                var response = await restClient.ExecuteAsync(request);

                var result = new ApiResponse(response.Content, response);

                return result;
            }
            catch (Exception ex)
            {
                throw new ListenApiException(ex.Message, ex);
            }
        }
    }
}
