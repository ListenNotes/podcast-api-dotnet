using PodcastAPI.Exceptions;
using System;
using System.Collections.Generic;

namespace PodcastAPI
{
    public class Sample
    {
        public static void Main()
        {
            try
            {

                var apiKey = Environment.GetEnvironmentVariable("LISTEN_API_KEY");

                var client = new Client(apiKey);
                var parameters = new Dictionary<string, string>();
                parameters.Add("q", "startup");
                var result = client.Search(parameters).Result;
                var jsonObject = result.ToJSON<dynamic>();
                Console.WriteLine($"Json Object: {jsonObject}");

                var freeQuota = result.GetFreeQuota();
                Console.WriteLine($"Free Quota: {freeQuota}");

                var usage = result.GetUsage();
                Console.WriteLine($"Usage: {usage}");

                var nextBillingDate = result.GetNextBillingDate();
                Console.WriteLine($"Next Billing Date: {nextBillingDate}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("page", "2");
                //result = client.FetchBestPodcasts(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "4d3fe717742d4963a85562e9f84d8c79");
                //result = client.FetchPodcastById(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "6b6d65930c5a4f71b254465871fed370");
                //result = client.FetchEpisodeById(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("ids", "c577d55b2b2b483c969fae3ceb58e362,0f34a9099579490993eec9e8c8cebb82");
                //result = client.BatchFetchEpisodes(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("ids", "3302bc71139541baa46ecb27dbf6071a,68faf62be97149c280ebcc25178aa731");
                //result = client.BatchFetchPodcasts(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "SDFKduyJ47r");
                //result = client.FetchCuratedPodcastsListById(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("page", "2");
                //result = client.FetchCuratedPodcastsLists(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("top_level_only", "1");
                //result = client.FetchPodcastGenres(parameters).Result;
                //Console.WriteLine($"{result}");

                //result = client.FetchPodcastRegions().Result;
                //Console.WriteLine($"{result}");

                //result = client.FetchPodcastLanguages().Result;
                //Console.WriteLine($"{result}");

                //result = client.JustListen().Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "25212ac3c53240a880dd5032e547047b");
                //parameters.Add("safe_mode", "1");
                //result = client.FetchRecommendationsForPodcast(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "914a9deafa5340eeaa2859c77f275799");
                //parameters.Add("safe_mode", "1");
                //result = client.FetchRecommendationsForEpisode(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "m1pe7z60bsw");
                //parameters.Add("type", "podcast_list");
                //result = client.FetchPlaylistById(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("page", "1");
                //result = client.FetchMyPlaylists(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("rss", "https://feeds.megaphone.fm/committed");
                //result = client.SubmitPodcast(parameters).Result;
                //Console.WriteLine($"{result}");

                //parameters = new Dictionary<string, string>();
                //parameters.Add("id", "4d3fe717742d4963a85562e9f84d8c79");
                //parameters.Add("reason", "the podcaster wants to delete it");
                //result = client.DeletePodcast(parameters).Result;
                //Console.WriteLine($"{result}");
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine($"Authentication Issue: {ex.Message}");
            }
            catch (InvalidRequestException ex)
            {
                Console.WriteLine($"Invalid Request: {ex.Message}");
            }
            catch (RateLimitException ex)
            {
                Console.WriteLine($"Rate Limit: {ex.Message}");
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine($"Not Found: {ex.Message}");
            }
            catch (ListenApiException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application Exception: {ex}");
            }            
        }
    }
}
