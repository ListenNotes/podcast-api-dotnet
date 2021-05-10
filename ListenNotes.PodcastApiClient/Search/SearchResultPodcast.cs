using Newtonsoft.Json;
using System.Collections.Generic;

namespace ListenNotes.PodcastApiClient
{
    [JsonObject]
    public class SearchResultPodcast
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("genre_ids")]
        public IEnumerable<int> GenreIds { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("listen_score")]
        public int? ListenScore { get; set; }

        [JsonProperty("title_original")]
        public string TitleOriginal { get; set; }

        [JsonProperty("listennotes_url")]
        public string ListenNotesUrl { get; set; }

        [JsonProperty("title_highlighted")]
        public string TitleHighlighted { get; set; }

        [JsonProperty("publisher_original")]
        public string PublisherOriginal { get; set; }

        [JsonProperty("publisher_highlighted")]
        public string PublisherHighlighted { get; set; }

        [JsonProperty("listen_score_global_rank")]
        public string ListenScoreGlobalRank { get; set; }
    }
}
