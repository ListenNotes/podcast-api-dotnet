using Newtonsoft.Json;
using System.Collections.Generic;

namespace ListenNotes.PodcastApiClient
{
    [JsonObject]
    public class SearchResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("rss")]
        public string Rss { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("audio")]
        public string Audio { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("podcast")]
        public SearchResultPodcast Podcast { get; set; }

        [JsonProperty("itunes_id")]
        public int ITunesId { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("pub_date_ms")]
        public long PubDateMs { get; set; }

        [JsonProperty("guid_from_rss")]
        public string GuidFromRss { get; set; }

        [JsonProperty("title_original")]
        public string TitleOriginal { get; set; }

        [JsonProperty("listennotes_url")]
        public string ListenNotesUrl { get; set; }

        [JsonProperty("audio_length_sec")]
        public string AudioLengthSeconds { get; set; }

        [JsonProperty("explicit_content")]
        public bool ExplicitContent { get; set; }

        [JsonProperty("title_highlighted")]
        public string TitleHighlighted { get; set; }

        [JsonProperty("description_original")]
        public string DescriptionOriginal { get; set; }

        [JsonProperty("description_highlighted")]
        public string DescriptionHighlighted { get; set; }

        [JsonProperty("transcripts_highlighted")]
        public IEnumerable<string> TranscriptsHighlighted { get; set; }

    }
}
