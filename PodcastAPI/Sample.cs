using System;
using System.Collections.Generic;

namespace ListenNotes.PodcastApiClient
{
    public class Sample
    {
        public static void Main()
        {
            try
            {
                var client = new Client();
                var parameters = new Dictionary<string, string>();

                parameters.Add("q", "Test");

                var result = client.Search(parameters).Result;

                var jsonObject = result.ToJSON<object>();

                var freeQuota = result.GetFreeQuota();

                var usage = result.GetUsage();

                var nextBillingDate = result.GetNextBillingDate();

                Console.Read();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.Read(); 
        }
    }
}
