using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
