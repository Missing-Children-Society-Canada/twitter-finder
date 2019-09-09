using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MCSC
{
    public static class ScrapeFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("scrape")]
        [FunctionName("ScrapeFunction")]
        public static async Task<LuisInput> Run([QueueTrigger("twitter")]string json, ILogger log)
        {
            log.LogInformation($"Scrape function invoked: {json}");

            var tweet = JsonConvert.DeserializeObject<Tweet>(json);

            var result = new LuisInput
            {
                SourceUrl = tweet.SourceUrl,
                TwitterProfileUrl = tweet.TwitterProfileURL,
                TweetUrl = tweet.TweetUrl
            };

            if(!string.IsNullOrEmpty(tweet.SourceUrl))
            {
                log.LogInformation($"Loading external reference into scraper '{tweet.SourceUrl}'.");

                var incident = await GetIncidentFromUrl(log, tweet.SourceUrl);
                if (incident != null)
                {
                    result.ShortSummary = incident.ShortSummary;
                    result.Summary = incident.Summary;
                }
                else
                {
                    log.LogWarning($"Reference failed to load content from '{result.SourceUrl}'.");
                }
            }
            else
            {
                log.LogInformation("No source url was available for this input, skipping scrape.");
                string text = StringSanitizer.SimplifyHtmlEncoded(tweet.TweetText);

                result.Summary = StringSanitizer.RemoveDoublespaces(text);
                
                result.ShortSummary = 
                    StringSanitizer.RemoveDoublespaces(
                    StringSanitizer.RemoveUrls(
                    StringSanitizer.RemoveHashtags(text)));
            }
            
            return result; 
        }

        private static async Task<Incident> GetIncidentFromUrl(ILogger log, string url)
        {
            //use smart reference first
            var smartReference = new SmartReference(url, log);
            var incident = await smartReference.LoadAsync();
            if (incident != null)
            {
                return incident;
            }

            log.LogWarning($"Smart reference failed to load content from '{url}'.");

            //fallback 
            var reference = new Reference(url, log);
            incident = await reference.LoadAsync();
            return incident;
        }
    }
}