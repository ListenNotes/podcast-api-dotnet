using Newtonsoft.Json;
using RestSharp;
using System.Linq;

namespace PodcastAPI
{
    public sealed class ApiResponse
    {
        private readonly IRestResponse Response;
        private readonly string jsonString;

        public ApiResponse(string jsonString, IRestResponse response)
        {
            Response = response;
            this.jsonString = jsonString;
        }

        public T ToJSON<T>()
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public override string ToString()
        {
            return jsonString;
        }

        public int GetFreeQuota()
        {
            var name = "x-listenapi-freequota";
            var headerField = Response.Headers.FirstOrDefault(h => h.Name.Equals(name)).Value?.ToString();

            if (headerField != null && int.TryParse(headerField, out int result))
            {
                return result;
            }

            throw new System.Exception($"Cannot get header {name}");
        }

        public int GetUsage()
        {
            var name = "x-listenapi-usage";
            var headerField = Response.Headers.FirstOrDefault(h => h.Name.Equals(name)).Value?.ToString();

            if (headerField != null && int.TryParse(headerField, out int result))
            {
                return result;
            }

            throw new System.Exception($"Cannot get header {name}");
        }

        public string GetNextBillingDate()
        {
            var name = "x-listenapi-nextbillingdate";
            var headerField = Response.Headers.FirstOrDefault(h => h.Name.Equals(name));

            return headerField?.Value?.ToString();
        }
    }
}
