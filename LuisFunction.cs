using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MCSC.Classes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MCSC
{
    public static class LuisFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("luis")]
        [FunctionName("LuisFunction")]
        public static async Task<string> Run([QueueTrigger("scrape")]string luisInputs, ILogger logger)
        {
            logger.LogInformation($"Luis function invoked: {luisInputs}");

            var listofInputs = JsonConvert.DeserializeObject<List<LuisInput>>(luisInputs);
            IList<MissingChild> luisResults = new List<MissingChild>();
            
            LuisHelper luis = new LuisHelper();
            foreach (var luisInput in listofInputs)
            {
                string shortSummary = luisInput.ShortSummary;
                
                MissingChild missingChild = FillInformationForMissingChild(luisInput);
                if (!string.IsNullOrEmpty(shortSummary))
                {
                    shortSummary = shortSummary.Replace("&","");
                    // take only first 500 characters so LUIS can handle it 
                    shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 498));

                    var luisResult = await LuisHelper.GetLuisResult(shortSummary);
                    if (luisResult != null)
                    {
                        var entityKeys = string.Join(",", luisResult.Entities.Select(s => s.Type + "=" + s.EntityFound));
                        logger.LogInformation($"luis returned the following entities:{entityKeys}");

                        MapLuisResultToMissingChild(missingChild, luisResult);
                    }
                    else
                    {
                        logger.LogWarning("luis did not return a result to process");
                    }
                }

                luisResults.Add(missingChild);
            }

            return JsonConvert.SerializeObject(luisResults);
        }

        private static MissingChild FillInformationForMissingChild(LuisInput luisInput)
        {
            return new MissingChild{
                    SourceUrl = luisInput.SourceUrl,
                    TweetUrl = luisInput.TweetUrl,
                    TwitterProfileUrl = luisInput.TwitterProfileUrl,
                    Summary = luisInput.Summary,
                    ShortSummary = luisInput.ShortSummary
            };
        }

        private static void MapLuisResultToMissingChild(MissingChild missingChild, LuisResult luisResult)
        {
            foreach (var entity in luisResult.Entities)
            {
                switch (entity.Type)
                {
                    case "Name":
                        missingChild.Name = entity.EntityFound;
                        break;
                    case "City":
                        missingChild.City = entity.EntityFound;
                        break;
                    case "Province":
                        missingChild.Province = entity.EntityFound;
                        break;
                    case "Age":
                        missingChild.Age = int.Parse(entity.EntityFound);
                        break;
                    case "Gender":
                        missingChild.Gender = entity.EntityFound;
                        break;
                    case "Ethnicity":
                        missingChild.Ethnicity = entity.EntityFound;
                        break;
                    case "MissingSince":
                        missingChild.MissingSince = entity.EntityFound;
                        break;
                    case "Height":
                        missingChild.Height = entity.EntityFound;
                        break;
                    case "Weight":
                        missingChild.Weight = entity.EntityFound;
                        break;
                    case "Found":
                        missingChild.Found = int.Parse(entity.EntityFound);
                        break;
                }
            }
        }
    }
}