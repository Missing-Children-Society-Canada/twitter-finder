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
using Microsoft.WindowsAzure.Storage;

namespace MCSC
{
    public static class TwitterFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("twitter")]
        [FunctionName("TwitterFunction")]
        public static async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var tweets =  JsonConvert.DeserializeObject<List<Tweet>>(requestBody);

            try
            {
                tweets = FilterMissingTweets(tweets);
                log.LogInformation($"Number of tweets with the 'missing' word: {tweets.Count}");
                if (tweets.Count == 0)
                {
                    return null;
                }

                if (!CloudStorageAccount.TryParse(Utils.GetEnvVariable("BlobStorageConnectionString"),
                    out var storageAccount))
                {
                    throw new Exception("unable to create storage account connection");
                }
                var blobReference = storageAccount.CreateCloudBlobClient()
                    .GetContainerReference(Utils.GetEnvVariable("BlobStorageContainerName"))
                    .GetBlockBlobReference(Utils.GetEnvVariable("BlobStorageBlobName"));

                List<Tweet> tweetsFromStorage;
                if (await blobReference.ExistsAsync())
                {
                    string jsonString = await blobReference.DownloadTextAsync();
                    tweetsFromStorage = JsonConvert.DeserializeObject<List<Tweet>>(jsonString);
                }
                else
                {
                    tweetsFromStorage = new List<Tweet>();
                }
                
                var newTweets = new List<Tweet>();
                foreach (var tweet in tweets)
                {
                    int index = tweetsFromStorage.FindIndex(f => f.TweetId == tweet.TweetId);
                    if (index < 0)
                    {
                        var formattedTweet = await FormatTweetAsync(tweet, log);
                        tweetsFromStorage.Add(formattedTweet);
                        newTweets.Add(formattedTweet);
                    }
                }
                
                log.LogInformation($"Duplicate check completed, number of new tweets {newTweets.Count}");
                if (newTweets.Count == 0)
                {
                    return null;
                }
                
                await blobReference.UploadTextAsync(JsonConvert.SerializeObject(tweetsFromStorage));
                return JsonConvert.SerializeObject(newTweets);
            }
            catch(Exception e)
            {
                log.LogInformation(e.ToString());
                return null;
            }
        }

        private static async Task<Tweet> FormatTweetAsync(Tweet tweet, ILogger log)
        {
            tweet.TwitterProfileURL = $"https://twitter.com/{tweet.TweetedBy}";
            tweet.TweetUrl = $"{tweet.TwitterProfileURL}/status/{tweet.TweetId}";

            var links = tweet.TweetText.Split("\t\r\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                            || s.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase));

            // only use the link if it resolves to a domain other than twitter
            foreach (string link in links)
            {
                string expandedUrl;
                try
                {
                    expandedUrl = await ExpandUrlAsync(link);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Could not expand link " + link);
                    expandedUrl = string.Empty;
                }

                if (!string.IsNullOrEmpty(expandedUrl) &&
                    !expandedUrl.Contains("twitter.com", StringComparison.InvariantCultureIgnoreCase))
                {
                    tweet.SourceUrl = expandedUrl;
                    break;
                }
            }
            return tweet;
        }

        private static List<Tweet> FilterMissingTweets(IEnumerable<Tweet> tweets)
        {
            var filteredTweets = new List<Tweet>();
            foreach(var tweet in tweets)
            {
                if (tweet.TweetText.Contains("missing", StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredTweets.Add(tweet);
                }
            }
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
                        var redirectUri = response.Headers.Location;
                        if (!redirectUri.IsAbsoluteUri)
                        {
                            redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                        }
                        return await ExpandUrlAsync(redirectUri.ToString(), depth);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    return url;
                }
            }
        }
    }
}