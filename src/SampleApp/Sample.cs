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
                var client = new Client();
                var parameters = new Dictionary<string, string>();

                parameters.Add("q", "Test");

                var result = client.Search(parameters).Result;

                var jsonObject = result.ToJSON<object>();

                var freeQuota = result.GetFreeQuota();

                Console.WriteLine($"Free Quota: {freeQuota}");

                var usage = result.GetUsage();

                Console.WriteLine($"Usage: {usage}");

                var nextBillingDate = result.GetNextBillingDate();

                Console.WriteLine($"Next Billing Date: {nextBillingDate}");

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
