using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MCSC.Classes;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCSC
{
    public static class LuisFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("luis")]
        [FunctionName("LuisFunction")]
        public static async Task<string> Run([QueueTrigger("scrape")]string luisInputs, ILogger log)
        {
            var listofInputs = JsonConvert.DeserializeObject<List<LuisInput>>(luisInputs);
            IList<MissingChild> luisResults = new List<MissingChild>();
            
            LUISHelper luis = new LUISHelper();
            foreach (var luisInput in listofInputs)
            {
                string shortSummary = luisInput.ShortSummary;
                
                MissingChild missingChild = FillInformationForMissingChild(luisInput);
                if (!string.IsNullOrEmpty(shortSummary))
                {
                    shortSummary = shortSummary.Replace("&","");
                    // take only first 500 characters so LUIS can handle it 
                    shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 499));
                    await luis.GetLuisResult(shortSummary, missingChild);
                }

                luisResults.Add(missingChild);
            }

            return JsonConvert.SerializeObject(luisResults);
        }

        static MissingChild FillInformationForMissingChild(LuisInput luisInput)
        {
            return new MissingChild{
                    SourceUrl = luisInput.SourceUrl,
                    TweetUrl = luisInput.TweetUrl,
                    TwitterProfileUrl = luisInput.TwitterProfileUrl,
                    Summary = luisInput.Summary,
                    ShortSummary = luisInput.ShortSummary
            };
        }
    }
}