using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using MCSC.Scrape;

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
                    var incident = await smartReference.LoadAsync();
                    if (incident != null)
                    {
                        luisInput.ShortSummary = incident.ShortSummary;
                        luisInput.Summary = incident.Summary;
                    }
                    else
                    {
                        log.LogWarning($"Smart reference failed to load content from '{luisInput.SourceUrl}'.");

                        var reference = new Reference(luisInput.SourceUrl, log);
                        incident = await reference.LoadAsync();
                        if (incident != null)
                        {
                            luisInput.ShortSummary = incident.ShortSummary;
                            luisInput.Summary = incident.Summary;
                        }
                        else
                        {
                            log.LogWarning($"Reference failed to load content from '{luisInput.SourceUrl}'.");
                        }
                    }
                }
                else
                {
                    log.LogInformation("No source url was available for this input, skipping scrape.");
                    luisInput.Summary = StringSanitizer.RemoveHashtags(tweet.TweetText);

                    luisInput.ShortSummary = StringSanitizer.RemoveFillerWords(
                        StringSanitizer.RemoveHashtags(tweet.TweetText));
                }
                
                scrapedTweets.Add(luisInput);
            }
            return JsonConvert.SerializeObject(scrapedTweets); 
        }
    }
}