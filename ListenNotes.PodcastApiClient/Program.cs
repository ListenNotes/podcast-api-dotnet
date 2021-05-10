using System;

namespace ListenNotes.PodcastApiClient
{
    public class Program
    {
        public static void Main()
        {
            var client = new PodcastApiClient(null, true);
            var searchOptions = new SearchOptions
            {
                Query = "Test"
            };

            var result = client.Search(searchOptions).Result;

            Console.Read();
        }
    }
}
