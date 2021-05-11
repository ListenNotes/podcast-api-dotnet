using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        }

        [TestMethod]
        public void Client_ShouldUseProdUrlWithApiKey()
        {
            // Arrange & Act
            var apiKey = "testApiKey";

            var client = new Client(apiKey);

            // Assert
            Assert.AreEqual(client.restClient.BaseUrl, client.BASE_URL_PROD);
        }
    }
}
