using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MCSC.Classes;

namespace MCSC
{
    public static class TwitterFunction
    {
        private static readonly StorageHelper storageHelper = new StorageHelper();

        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("twitter")]
        [FunctionName("TwitterFunction")]
        
        public static async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            storageHelper.InitializeStorageAccount();

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var tweets =  JsonConvert.DeserializeObject<List<Tweet>>(requestBody);
            
            try
            {
                tweets = await FormatTweetsAsync(tweets);
                log.LogInformation($"Number of tweets with the 'missing' word: {tweets.Count}");

                log.LogInformation($"Updating and looking for duplicated tweets list in Storage");
                tweets = await CheckDuplicatesInStorageAsync(tweets);
                log.LogInformation($"Update completed, number of unique tweets {tweets.Count}");

                if(tweets.Count>0)
                    return JsonConvert.SerializeObject(tweets);
                return null;
            }
            catch(Exception e)
            {
                log.LogInformation(e.ToString());
                return null;
            }
        }

        private static async Task<List<Tweet>> FormatTweetsAsync(List<Tweet> tweets)
        {
            var filteredTweets = new List<Tweet>();
            foreach(var tweet in tweets)
            {
                if(tweet.TweetText.Contains("missing", StringComparison.InvariantCultureIgnoreCase))
                {
                    tweet.TwitterProfileURL = $"https://twitter.com/{tweet.TweetedBy}";
                    tweet.TweetUrl = $"{tweet.TwitterProfileURL}/status/{tweet.TweetId}";

                    var links = tweet.TweetText.Split("\t\r\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Where(s => s.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                                    || s.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase)
                                    || s.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase));

                    // only use the link if it resolves to a domain other than twitter
                    foreach (string link in links)
                    {
                        string expandedUrl = await ExpandUrlAsync(link);
                        if (!string.IsNullOrEmpty(expandedUrl))
                        {
                            // If Twitter is the source domain then don't bother parsing
                            if (!expandedUrl.Contains("twitter.com", StringComparison.InvariantCultureIgnoreCase))
                            {
                                tweet.SourceUrl = expandedUrl;
                                break;
                            }
                        }
                    }

                    filteredTweets.Add(tweet);
                }
            }
            filteredTweets.OrderByDescending(n => n.CreatedAtIso);
            return filteredTweets;
        }
        
        private static async Task<string> ExpandUrlAsync(string url, int depth = 0)
        {
            using (var handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Head
                };

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
                        var location = response.Headers.Location.ToString();
                        return await ExpandUrlAsync(location, depth);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    return url;
                }
            }
        }

        private static async Task<List<Tweet>> CheckDuplicatesInStorageAsync(List<Tweet> tweets)
        {
            var filteredTweets = new List<Tweet>();
            var tweetsFromStorage = await storageHelper.GetListOfTweetsAsync();
            foreach(var tweet in tweets)
            {
                int index = tweetsFromStorage.FindIndex(f => f.TweetId == tweet.TweetId);
            
                if (index<0) 
                {
                    tweetsFromStorage.Add(tweet);
                    filteredTweets.Add(tweet);
                }
            }
            await storageHelper.UpdateBlobAsync(JsonConvert.SerializeObject(tweetsFromStorage));
            return filteredTweets;
        }
    }
}