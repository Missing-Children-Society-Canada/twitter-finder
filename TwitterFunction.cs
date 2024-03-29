using System;
using System.IO;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace MCSC
{
    /// <summary>
    /// Process source tweet list, look for any unique entries that mention missing persons
    /// </summary>
    public static class TwitterFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [FunctionName("TwitterFunction")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Queue("%QueueName_TwitterOutput%")] ICollector<TweetModel> queueCollector,
            ILogger log)
        {
            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var container =  JsonConvert.DeserializeObject<MCSC.V2.ContainerModel>(requestBody);
            var tweets = container.ConvertToArchived();

            try
            {
                tweets = FilterMissingTweets(tweets);
                log.LogInformation($"Number of tweets matching the keywords: {tweets.Count}");
                if (tweets.Count == 0)
                {
                    return;
                }

                // order the tweets by date
                tweets.Sort(new TweetDateComparer());

                if (!CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable("BlobStorageConnectionString", EnvironmentVariableTarget.Process),
                    out var storageAccount))
                {
                    throw new Exception("unable to create storage account connection");
                }
                var blobReference = storageAccount.CreateCloudBlobClient()
                    .GetContainerReference(Environment.GetEnvironmentVariable("BlobStorageContainerName", EnvironmentVariableTarget.Process))
                    .GetBlockBlobReference(Environment.GetEnvironmentVariable("BlobStorageBlobName", EnvironmentVariableTarget.Process));

                List<TweetModel> tweetsFromStorage;
                if (await blobReference.ExistsAsync())
                {
                    string jsonString = await blobReference.DownloadTextAsync();
                    tweetsFromStorage = JsonConvert.DeserializeObject<List<TweetModel>>(jsonString);
                }
                else
                {
                    tweetsFromStorage = new List<TweetModel>();
                }

                int newTweetsCount = 0;
                foreach (var tweet in tweets)
                {
                    // if this is a re-tweet then check to see if we've already processed the original
                    if (tweet.OriginalTweet != null &&
                        tweetsFromStorage.FindIndex(f => f.TweetId == tweet.OriginalTweet.TweetId) >= 0)
                        continue;

                    // skip the tweet if it was already processed
                    if (tweetsFromStorage.FindIndex(f => f.TweetId == tweet.TweetId) >= 0)
                        continue;

                    tweetsFromStorage.Add(tweet);
                    queueCollector.Add(tweet);
                    newTweetsCount++;
                }
                log.LogInformation($"Duplicate check completed, number of new tweets {newTweetsCount}");
                
                if (newTweetsCount > 0)
                {
                    // Before we upload the processed tweets, let's trim down some old data - anything older than a year
                    int removedTweets = tweetsFromStorage.RemoveAll(w => w.CreatedAtIso < DateTime.Now.AddYears(-1));

                    log.LogInformation($"Removed {removedTweets} old tweet(s) from the processed tweets.json file.");

                    await blobReference.UploadTextAsync(JsonConvert.SerializeObject(tweetsFromStorage));
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Error in twitter function.");
                throw;
            }
        }
        
        private static List<TweetModel> FilterMissingTweets(IEnumerable<TweetModel> tweets)
        {
            var filteredTweets = new List<TweetModel>();
            var keywordsList = Environment.GetEnvironmentVariable("TweetKeywords", EnvironmentVariableTarget.Process);
            var keywords = keywordsList.Split(",");

            foreach(var tweet in tweets)
            {
                if (keywords.Any(s => tweet.TweetText.Contains(s, StringComparison.InvariantCultureIgnoreCase))) {
                    filteredTweets.Add(tweet);
                }
            }
            return filteredTweets;
        }
    }

    public class TweetDateComparer : IComparer<TweetModel>
    {
        public int Compare(TweetModel x, TweetModel y)
        {
            if (x == null || y == null)
            {
                return 0;
            }
            return x.CreatedAtIso.CompareTo(y.CreatedAtIso);
        }
    }
}