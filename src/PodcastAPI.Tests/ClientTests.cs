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
        public void Client_ShouldUseTestUrlWithNullApiKey()
        {
            // Arrange & Act
            var client = new Client();

            // Assert
            Assert.AreEqual(client.restClient.BaseUrl, client.BASE_URL_TEST);
            Assert.AreEqual(null, client.restClient.DefaultParameters.FirstOrDefault(p => p.Name.Equals("X-ListenAPI-Key"))?.Value);
        }

        [TestMethod]
        public void Client_ShouldUseProdUrlWithApiKey()
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
        public async Task Client_ShouldThrowAuthenticationExceptionWithInvalidApiKey()
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
        public void Client_ShouldHave30SecondDefaultTimeout()
        {
            // Arrange & Act
            var client = new Client();

            // Assert
            Assert.AreEqual(client.restClient.Timeout, 30000);
        }

        [TestMethod]
        public void Client_SearchMockDataShouldExist()
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
        public void Client_TypeaheadMockDataShouldExist()
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
        public void Client_BestPodcastsMockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("genre_id", "23");

            // Act
            var result = client.BestPodcasts(parameters).Result;

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
        public void Client_GenresMockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            var parameters = new Dictionary<string, string>();

            parameters.Add("top_level_only", "1");

            // Act
            var result = client.Genres(parameters).Result;

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
        public void Client_RegionsMockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            // Act
            var result = client.Regions().Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.regions is JObject);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/regions");
        }

        [TestMethod]
        public void Client_LanguagesMockDataShouldExist()
        {
            // Arrange
            var client = new Client();

            // Act
            var result = client.Languages().Result;

            // Assert
            Assert.AreEqual(result.response.StatusCode, System.Net.HttpStatusCode.OK);

            var json = result.ToJSON<dynamic>();

            Assert.IsTrue(json.languages is IEnumerable);
            Assert.IsTrue(json.languages.Count > 0);

            Assert.AreEqual(result.response.Request.Method, RestSharp.Method.GET);
            Assert.AreEqual(result.response.ResponseUri.AbsolutePath, "/api/v2/languages");
        }
    }
}
