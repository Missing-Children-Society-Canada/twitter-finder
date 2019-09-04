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

            var luisInput = new LuisInput
            {
                SourceUrl = tweet.SourceUrl,
                TwitterProfileUrl = tweet.TwitterProfileURL,
                TweetUrl = tweet.TweetUrl
            };

            if(!string.IsNullOrEmpty(luisInput.SourceUrl))
            {
                log.LogInformation($"Loading external reference into scraper '{luisInput.SourceUrl}'.");

                var incident = await GetIncidentFromUrl(log, luisInput);
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
            else
            {
                log.LogInformation("No source url was available for this input, skipping scrape.");
                string text = StringSanitizer.SimplifyHtmlEncoded(tweet.TweetText);

                luisInput.Summary = StringSanitizer.RemoveDoublespaces(text);
                
                luisInput.ShortSummary = 
                    StringSanitizer.RemoveDoublespaces(
                    StringSanitizer.RemoveUrls(
                    StringSanitizer.RemoveHashtags(text)));
            }
            
            return luisInput; 
        }

        private static async Task<Incident> GetIncidentFromUrl(ILogger log, LuisInput luisInput)
        {
            //use smart reference first
            var smartReference = new SmartReference(luisInput.SourceUrl, log);
            var incident = await smartReference.LoadAsync();
            if (incident != null)
            {
                return incident;
            }

            log.LogWarning($"Smart reference failed to load content from '{luisInput.SourceUrl}'.");

            //fallback 
            var reference = new Reference(luisInput.SourceUrl, log);
            incident = await reference.LoadAsync();
            return incident;
        }
    }
}