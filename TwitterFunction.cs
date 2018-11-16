using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using MCSC.Classes;

namespace MCSC
{
    public static class TwitterFunction
    {
        private static StorageHelper storageHelper= new StorageHelper();
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("twitter")]
        [FunctionName("TwitterFunction")]
        
        public static string Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            storageHelper.InitializeStorageAccount();

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            var tweets =  JsonConvert.DeserializeObject<List<Tweet>>(requestBody);
            
            try
            {
                tweets = FormatTweets(tweets);
                log.LogInformation($"Number of tweets with the 'missing' word: {tweets.Count}");

                log.LogInformation($"Updating and looking for duplicated tweets list in Storage");
                tweets = CheckDuplicatesInStorage(tweets);
                log.LogInformation($"Update completed");

                if(tweets.Count>0) return JsonConvert.SerializeObject(tweets);
                else return null;
            }
            catch(Exception e)
            {
                log.LogInformation(e.ToString());
                return null;
            }
        }

        static List<Tweet> FormatTweets(List<Tweet> tweets)
        {
            var filteredTweets = new List<Tweet>();
            foreach(var tweet in tweets)
            {
                if(tweet.TweetText.ToLower().Contains("missing"))
                {
                    tweet.TwitterProfileURL = $"https://twitter.com/{tweet.TweetedBy}";
                    tweet.TweetUrl = $"{tweet.TwitterProfileURL}/status/{tweet.TweetId}";
                    var stringSplitOptions = StringSplitOptions.RemoveEmptyEntries;
                    var links = tweet.TweetText.Split("\t\n ".ToCharArray(), stringSplitOptions)
                        .Where(s => s.StartsWith("http://") 
                        || s.StartsWith("www.") 
                        || s.StartsWith("https://"));

                    foreach (string link in links)
                        tweet.SourceUrl = link;

                    filteredTweets.Add(tweet);
                }
            }
            filteredTweets.OrderByDescending(n => n.CreatedAtIso);
            return filteredTweets;
        }

        static List<Tweet> CheckDuplicatesInStorage(List<Tweet> tweets)
        {
            var filteredTweets = new List<Tweet>();
            var tweetsFromStorage = storageHelper.GetListOfTweetsAsync().Result;
            foreach(var tweet in tweets)
            {
                int index = tweetsFromStorage.FindIndex(f => f.TweetId == tweet.TweetId);
            
                if (index<0) 
                {
                    tweetsFromStorage.Add(tweet);
                    filteredTweets.Add(tweet);
                }
            }
            storageHelper.UpdateBlobAsync(JsonConvert.SerializeObject(tweetsFromStorage));
            return filteredTweets;
        }
    }
}