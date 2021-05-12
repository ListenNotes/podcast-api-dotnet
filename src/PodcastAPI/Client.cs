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
        public readonly string userAgent;

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

            // default is 30 seconds
            restClient.Timeout = 30000;

            restClient.AddDefaultHeader("User-Agent", userAgent);
        }

        public async Task<ApiResponse> Search(IDictionary<string, string> parameters)
        {
            var url = "search";
            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetTypeahead(IDictionary<string, string> parameters)
        {
            var url = "typeahead";
            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetBestPodcasts(IDictionary<string, string> parameters)
        {
            var url = "best_podcasts";
            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetGenres(IDictionary<string, string> parameters)
        {
            var url = "genres";
            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetRegions()
        {
            var url = "regions";
            return await Get(url);
        }

        public async Task<ApiResponse> GetLanguages()
        {
            var url = "languages";
            return await Get(url);
        }

        public async Task<ApiResponse> GetPodcast(IDictionary<string, string> parameters)
        {
            var url = $"podcasts/{parameters["id"]}";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetEpisode(IDictionary<string, string> parameters)
        {
            var url = $"episodes/{parameters["id"]}";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetCuratedPodcast(IDictionary<string, string> parameters)
        {
            var url = $"curated_podcasts/{parameters["id"]}";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetCuratedPodcasts(IDictionary<string, string> parameters)
        {
            var url = "curated_podcasts";
            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetJustListen()
        {
            var url = $"just_listen";

            return await Get(url);
        }

        public async Task<ApiResponse> GetPodcastRecommendations(IDictionary<string, string> parameters)
        {
            var url = $"podcasts/{parameters["id"]}/recommendations";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetEpisodeRecommendations(IDictionary<string, string> parameters)
        {
            var url = $"episodes/{parameters["id"]}/recommendations";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetPlaylist(IDictionary<string, string> parameters)
        {
            var url = $"playlists/{parameters["id"]}";
            parameters.Remove("id");

            return await Get(url, parameters);
        }

        public async Task<ApiResponse> GetPlaylists(IDictionary<string, string> parameters)
        {
            var url = "playlists";
            return await Get(url, parameters);
        }

        private async Task<ApiResponse> Get(string url, IDictionary<string, string> queryParameters = null)
        {
            var request = new RestRequest(url, Method.GET);

            if (queryParameters != null)
            {
                foreach (var parameter in queryParameters)
                {
                    request.AddQueryParameter(parameter.Key, parameter.Value);
                }
            }

            var response = await restClient.ExecuteAsync(request);

            ProcessStatus((int)response.StatusCode);

            var result = new ApiResponse(response.Content, response);

            return result;
        }

        private async Task<ApiResponse> Post(string url, IDictionary<string, string> bodyParameters)
        {
            var request = new RestRequest(url, Method.POST);

            request.AddJsonBody(bodyParameters);

            var response = await restClient.ExecuteAsync(request);

            ProcessStatus((int)response.StatusCode);

            var result = new ApiResponse(response.Content, response);

            return result;
        }

        private async Task<ApiResponse> Delete(string url)
        {
            var request = new RestRequest(url, Method.DELETE);

            var response = await restClient.ExecuteAsync(request);

            ProcessStatus((int)response.StatusCode);

            var result = new ApiResponse(response.Content, response);

            return result;
        }

        private void ProcessStatus(int status)
        {
            switch (status)
            {
                case 401:
                    throw new AuthenticationException("Wrong api key or your account is suspended");
                case 429:
                    throw new RateLimitException("You use FREE plan and you exceed the quota limit.");
                case 404:
                    throw new NotFoundException("Endpoint not exist, or podcast / episode not exist.");
                case 400:
                    throw new InvalidRequestException("Something wrong on your end (client side errors), e.g., missing required parameters.");
                case 0:
                    throw new ApiConnectionException("Failed to connect to Listen API servers.");
            }

            if (status >= 500)
            {
                throw new ListenApiException("Error on our end (unexpected server errors)");
            }
        }
    }
}
