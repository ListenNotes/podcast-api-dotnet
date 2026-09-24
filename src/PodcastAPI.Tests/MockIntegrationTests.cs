using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PodcastAPI.Tests;

[TestClass]
[TestCategory("Integration")]
public class MockIntegrationTests
{
    [TestMethod]
    public async Task AllMethodsReachOnlyThePublicMockWithoutCredentials()
    {
        if (Environment.GetEnvironmentVariable("LISTEN_API_MOCK_INTEGRATION") != "1")
        {
            Assert.Inconclusive("Opt in with LISTEN_API_MOCK_INTEGRATION=1.");
            return;
        }
        // No environment API key or base URL; redirects are disabled by Client.
        using var client = new Client();
        Assert.AreEqual("https://listen-api-test.listennotes.com/api/v2/", client.BaseUrl.AbsoluteUri);
        foreach (var op in ClientTests.Operations)
        {
            var operation = op.GetProperty("operationId").GetString()!;
            var response = await MethodDispatch.Call(operation, client, ClientTests.Examples(op));
            Assert.IsTrue((int)response.StatusCode is >= 200 and < 300, operation);
            var json = response.ToJSON<Newtonsoft.Json.Linq.JObject>();
            Assert.IsNotNull(json, operation);
            Assert.IsTrue(json.Count > 0, operation);
            if (operation == "deletePlaylistItem") Assert.AreEqual(true, (bool?)json["deleted"], operation);
            if (operation is "createPlaylist" or "updatePlaylist" or "addPlaylistItem" or "updatePlaylistItemNotes")
                Assert.IsNotNull(json["id"], operation);
        }
    }
}
