using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MCSC.Parsing;
using MCSC.Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
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
                var luisInput = new LuisInput
                {
                    SourceUrl = tweet.SourceUrl,
                    TwitterProfileUrl = tweet.TwitterProfileURL,
                    TweetUrl = tweet.TweetUrl
                };

                if(!string.IsNullOrEmpty(luisInput.SourceUrl))
                {
                    log.LogInformation($"Loading external reference into scraper '{luisInput.SourceUrl}'.");

                    //use smart reference first, if that fails fallback 
                    var smartReference = new SmartReference(luisInput.SourceUrl, log);
                    if (smartReference.Load(out var shortSummary, out var summary))
                    {
                        luisInput.ShortSummary = shortSummary;
                        luisInput.Summary = summary;
                    }
                    else
                    {
                        var reference = new Reference(luisInput.SourceUrl, log);
                        var incident = reference.Load();

                        luisInput.ShortSummary = incident.ShortSummary;
                        luisInput.Summary = incident.Summary;
                    }
                }
                else
                {
                    log.LogWarning("No source url was available for this input, skipping scrape.");
                }
                
                scrapedTweets.Add(luisInput);
            }
            return JsonConvert.SerializeObject(scrapedTweets); 
        }
    }
}