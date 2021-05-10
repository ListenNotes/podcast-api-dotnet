using Newtonsoft.Json;
using System.Collections.Generic;

namespace ListenNotes.PodcastApiClient
{
    [JsonObject]
    public class SearchResponse
    {
        [JsonProperty("took")]
        public float Took { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("results")]
        public IEnumerable<SearchResult> Results { get; set; }

        [JsonProperty("next_offset")]
        public float NextOffset { get; set; }
    }
}
