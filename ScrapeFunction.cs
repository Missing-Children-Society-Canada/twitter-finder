using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MCSC
{
    public static class ScrapeFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("%QueueName_ScrapeOutput%")]
        [FunctionName("ScrapeFunction")]
        public static async Task<LuisInput> Run([QueueTrigger("%QueueName_TwitterOutput%")]string json, ILogger log)
        {
            log.LogInformation($"Scrape function invoked:\n{json}");

            var tweet = JsonConvert.DeserializeObject<TweetModel>(json);

            string summary = null, shortSummary = null;
            var sourceUrl = await SourceUrlFromTweetAsync(tweet, log);
            if(!string.IsNullOrEmpty(sourceUrl))
            {
                log.LogInformation($"Loading external reference into scraper '{sourceUrl}'.");

                var incident = await GetIncidentFromUrl(log, sourceUrl);
                if (incident != null)
                {
                    shortSummary = incident.ShortSummary;
                    summary = incident.Summary;
                }
                else
                {
                    log.LogWarning($"Reference failed to load content from '{sourceUrl}'.");
                }
            }
            else
            {
                log.LogInformation("No source url was available for this input, skipping scrape.");
                string text = StringSanitizer.SimplifyPunctuation(
                    System.Net.WebUtility.HtmlDecode(tweet.TweetText));

                summary = StringSanitizer.RemoveDoublespaces(text);
                shortSummary = 
                    StringSanitizer.RemoveDoublespaces(
                    StringSanitizer.RemoveUrls(
                    StringSanitizer.RemoveHashtags(text)));
            }

            return new LuisInput
            {
                SourceUrl = sourceUrl,
                TwitterProfileUrl = $"https://twitter.com/{tweet.TweetedBy}",
                TweetUrl = $"https://twitter.com/{tweet.TweetedBy}/status/{tweet.TweetId}",
                UserLocation = tweet.UserDetails?.Location,
                ShortSummary = shortSummary,
                Summary = summary
            };
        }

        private static async Task<string> ExpandUrlAsync(ILogger log, string url, int depth = 0)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;
                
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Head
                };
                
				log.LogInformation($"URL Expansion ({depth}): {url}");

                using (var client = new HttpClient(handler))
                {
                    var response = await client.SendAsync(request);
                    var statusCode = (int)response.StatusCode;
                    
                    // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                    if (statusCode >= 300 && statusCode <= 399)
                    {
                        //exit if we've exceeded the max link depth, this is intended to stop infinite redirect loops
                        depth++;
                        if (depth > 5)
                        {
                            return null;
                        }
                        var redirectUri = response.Headers.Location;
                        if (!redirectUri.IsAbsoluteUri)
                        {
                            redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                        }
                        return await ExpandUrlAsync(log, redirectUri.ToString(), depth);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    return url;
                }
            }
        }

        private static async Task<string> SourceUrlFromTweetAsync(TweetModel tweet, ILogger log)
        {
            var links = tweet.TweetText.Split("\t\r\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                            || s.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase));
            string url = null;
            // only use the link if it resolves to a domain other than twitter
            foreach (string link in links)
            {
                string expandedUrl;
                try
                {
                    expandedUrl = await ExpandUrlAsync(log, link);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Could not expand link " + link);
                    expandedUrl = string.Empty;
                }

                if (!string.IsNullOrEmpty(expandedUrl) &&
                    !expandedUrl.Contains("twitter.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    url = expandedUrl;
                    break;
                }
            }
            return url;
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