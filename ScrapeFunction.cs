using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MCSC.Parsing;
using MCSC.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MCSC
{
    public static class ScrapeFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("scrape")]
        [FunctionName("ScrapeFunction")]
        public static async Task<string> Run([QueueTrigger("twitter")]string tweets, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {tweets}");
            
            var listOfTweets = JsonConvert.DeserializeObject<List<Tweet>>(tweets);

            var scrapedTweets = new List<LuisInput>();
            foreach(var tweet in listOfTweets)
            {
                var luisInput = new LuisInput()
                {
                    SourceUrl = tweet.SourceUrl,
                    TwitterProfileUrl = tweet.TwitterProfileURL,
                    TweetUrl = tweet.TweetUrl,
                };

                if(!string.IsNullOrEmpty(luisInput.SourceUrl))
                {
                    // If Twitter is the source url then don't bother parsing
                    if (!luisInput.SourceUrl.Contains("https://twitter.com"))
                    {
                        var reference = new Reference(luisInput.SourceUrl);
                        var incident = reference.Load();

                        luisInput.ShortSummary = incident.ShortSummary;
                        luisInput.Summary = incident.Summary;
                    }
                }
                
                scrapedTweets.Add(luisInput);
            }
            return JsonConvert.SerializeObject(scrapedTweets); 
        }
    }
}