using Newtonsoft.Json;

namespace ListenNotes.PodcastApiClient
{
    [JsonObject]
    public class SearchOptions
    {
        [JsonRequired]
        [JsonProperty("q")]
        public string Query { get; set; }

        [JsonProperty("sort_by_date")]
        public int SortByDate { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("offset")]
        public string Offset { get; set; }

        [JsonProperty("len_min")]
        public int MinimumLength { get; set; }

        [JsonProperty("len_max")]
        public int MaximumLength { get; set; }

        [JsonProperty("episode_count_min")]
        public int EpisodeCountMinimum { get; set; }

        [JsonProperty("episode_count_max")]
        public int EpisodeCountMaximum { get; set; }

        [JsonProperty("genre_ids")]
        public string GenreIds { get; set; }

        [JsonProperty("published_before")]
        public int PublishedBefore { get; set; }

        [JsonProperty("published_after")]
        public int PublishedAfter { get; set; }

        [JsonProperty("only_in")]
        public string OnlyIn { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("ocid")]
        public string Ocid { get; set; }

        [JsonProperty("ncid")]
        public string Ncid { get; set; }

        [JsonProperty("safe_mode")]
        public int SafeMode { get; set; }
    }
}
