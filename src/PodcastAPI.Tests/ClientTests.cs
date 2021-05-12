using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PodcastAPI.Exceptions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PodcastAPI.Tests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void Client_Constructor_ShouldUseTestUrlWithNullApiKey()
        {
            // Arrange & Act
            var client = new Client();

            // Assert
            Assert.AreEqual(client.restClient.BaseUrl, client.BASE_URL_TEST);
            Assert.AreEqual(null, client.restClient.DefaultParameters.FirstOrDefault(p => p.Name.Equals("X-ListenAPI-Key"))?.Value);
        }

        [TestMethod]
        public void Client_Constructor_ShouldUseProdUrlWithApiKey()
        {
            // Arrange
            var apiKey = "testApiKey";

            // Act
            var client = new Client(apiKey);

            // Assert
            Assert.AreEqual(client.restClient.BaseUrl, client.BASE_URL_PROD);
            Assert.AreEqual(apiKey, client.restClient.DefaultParameters.First(p => p.Name.Equals("X-ListenAPI-Key")).Value);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthenticationException))]
        public async Task Client_Constructor_ShouldThrowAuthenticationExceptionWithInvalidApiKey()
        {
            // Arrange
            var apiKey = "invalidApiKey";
            var parameters = new Dictionary<string, string>();
            parameters.Add("q", "test");

            // Act
            var client = new Client(apiKey);

            // Assert
            var result = await client.Search(parameters);
        }

        [TestMethod]
        public void Client_Constructor_ShouldHave30SecondDefaultTimeout()
        {
            // Arrange & Act
            var client = new Client();

            // Assert
            Assert.AreEqual(client.restClient.Timeout, 30000);
        }

        [TestMethod]
        public void Client_Search_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("q", "test");
            parameters.Add("sort_by_date", "1");

            // Act
            var result = client.Search(parameters).Result;
            
            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var results = result.ToJSON<dynamic>().results;

            Assert.IsTrue(results is IEnumerable);
            Assert.IsTrue(results.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/search");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("sort_by_date")).Value, parameters["sort_by_date"]);
            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("q")).Value, parameters["q"]);
        }

        [TestMethod]
        public void Client_Typeahead_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("q", "test");
            parameters.Add("show_podcasts", "1");

            // Act
            var result = client.Typeahead(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.terms is IEnumerable);
            Assert.IsTrue(json.terms.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/typeahead");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("show_podcasts")).Value, parameters["show_podcasts"]);
            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("q")).Value, parameters["q"]);
        }

        [TestMethod]
        public void Client_FetchBestPodcasts_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("genre_id", "23");

            // Act
            var result = client.FetchBestPodcasts(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.total > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/best_podcasts");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("genre_id")).Value, parameters["genre_id"]);
        }

        [TestMethod]
        public void Client_FetchPodcastGenres_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("top_level_only", "1");

            // Act
            var result = client.FetchPodcastGenres(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.genres is IEnumerable);
            Assert.IsTrue(json.genres.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/genres");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("top_level_only")).Value, parameters["top_level_only"]);
        }

        [TestMethod]
        public void Client_FetchPodcastRegions_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            // Act
            var result = client.FetchPodcastRegions().Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.regions is JObject);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/regions");
        }

        [TestMethod]
        public void Client_FetchPodcastLanguages_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            // Act
            var result = client.FetchPodcastLanguages().Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.languages is IEnumerable);
            Assert.IsTrue(json.languages.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/languages");
        }

        [TestMethod]
        public void Client_FetchPodcastById_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchPodcastById(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.episodes is IEnumerable);
            Assert.IsTrue(json.episodes.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/podcasts/{id}");
        }

        [TestMethod]
        public void Client_FetchEpisodeById_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchEpisodeById(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.podcast.rss is IEnumerable);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/episodes/{id}");
        }

        [TestMethod]
        public void Client_FetchCuratedPodcastsListById_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchCuratedPodcastsListById(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.podcasts is IEnumerable);
            Assert.IsTrue(json.podcasts.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/curated_podcasts/{id}");
        }

        [TestMethod]
        public void Client_JustListen_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            // Act
            var result = client.JustListen().Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.audio_length_sec > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/just_listen");
        }

        [TestMethod]
        public void Client_FetchCuratedPodcastsLists_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();
            parameters.Add("page", "2");

            // Act
            var result = client.FetchCuratedPodcastsLists(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.total > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/curated_podcasts");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("page")).Value, parameters["page"]);
        }

        [TestMethod]
        public void Client_FetchRecommendationsForPodcast_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchRecommendationsForPodcast(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.recommendations is IEnumerable);
            Assert.IsTrue(json.recommendations.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/podcasts/{id}/recommendations");
        }

        [TestMethod]
        public void Client_FetchRecommendationsForEpisode_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchRecommendationsForEpisode(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.recommendations is IEnumerable);
            Assert.IsTrue(json.recommendations.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/episodes/{id}/recommendations");
        }

        [TestMethod]
        public void Client_FetchMyPlaylists_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();
            parameters.Add("page", "2");

            // Act
            var result = client.FetchMyPlaylists(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.playlists is IEnumerable);
            Assert.IsTrue(json.playlists.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/playlists");

            var requestParameters = result.response.Request.Parameters;

            Assert.AreEqual(requestParameters.First(rp => rp.Name.Equals("page")).Value, parameters["page"]);
        }

        [TestMethod]
        public void Client_FetchPlaylistById_MockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            var id = "23";

            parameters.Add("id", id);

            // Act
            var result = client.FetchPlaylistById(parameters).Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.items is IEnumerable);
            Assert.IsTrue(json.items.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, $"/api/v2/playlists/{id}");
        }
    }
}
