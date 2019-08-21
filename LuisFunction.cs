using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            var listofInputs = JsonConvert.DeserializeObject<List<LuisInput>>(luisInputs);
            logger.LogInformation($"Luis function {listofInputs.Count} items, invoked: {luisInputs}");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Utils.GetEnvVariable("LUISsubscriptionKey"));

            IList<MissingChild> luisResults = new List<MissingChild>();
            foreach (var luisInput in listofInputs)
            {
                string shortSummary = luisInput.ShortSummary;
                
                MissingChild missingChild = FillInformationForMissingChild(luisInput);
                if (!string.IsNullOrEmpty(shortSummary))
                {
                    shortSummary = shortSummary.Replace("&","");
                    // take only first 500 characters so LUIS can handle it 
                    shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 498));

                    logger.LogInformation($"Sending LUIS query \"{shortSummary}\".");

                    var luisResult = await GetLuisResult(httpClient, shortSummary, logger);
                    if (luisResult != null)
                    {
                        var entityKeys = string.Join(",", luisResult.Entities.Select(s => s.Type + "=" + s.EntityFound));
                        logger.LogInformation($"LUIS returned the following entities:{entityKeys}");

                        var bestEntities = SelectBestEntities(luisResult.Entities);

                        MapLuisResultToMissingChild(missingChild, bestEntities, logger);
                    }
                    else
                    {
                        logger.LogWarning("LUIS did not return entities to process.");
                    }
                }
                else
                {
                    logger.LogInformation($"LUIS skipped tweet {luisInput.TweetUrl} due to missing short summary.");
                }

                luisResults.Add(missingChild);

                await Task.Delay(200);
            }

            return JsonConvert.SerializeObject(luisResults);
        }

        private static IEnumerable<Entity> SelectBestEntities(IEnumerable<Entity> inputList)
        {
            return inputList.GroupBy(item => item.Type)
                .Select(grp => grp.Aggregate((max, cur) => (max == null || cur.Score > max.Score) ? cur : max));
        }

        ///<summary>
        /// Returns a json string containing the results obtained 
        /// from the LUIS service defined in the env variables
        ///</summary>
        private static async Task<LuisResult> GetLuisResult(HttpClient httpClient, string shortSummary, ILogger logger)
        {
            string luisAppID = Utils.GetEnvVariable("LUISappID");
            string luisEndpoint = Utils.GetEnvVariable("LUISendpoint");

            string luisUri = $"{luisEndpoint}{luisAppID}?verbose=true&timezoneOffset=-360&q=\"{shortSummary}\"";
            var response = await httpClient.GetAsync(luisUri);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var contents = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LuisResult>(contents);
            }
            logger.LogError($"LUIS returned error status code {response.StatusCode}.");
            return null;
        }

        private static MissingChild FillInformationForMissingChild(LuisInput luisInput)
        {
            return new MissingChild
            {
                SourceUrl = luisInput.SourceUrl,
                TweetUrl = luisInput.TweetUrl,
                TwitterProfileUrl = luisInput.TwitterProfileUrl,
                Summary = luisInput.Summary,
                ShortSummary = luisInput.ShortSummary
            };
        }

        private static void MapLuisResultToMissingChild(MissingChild missingChild, IEnumerable<Entity> entities, ILogger logger)
        {
            foreach (var entity in entities)
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
                        if (int.TryParse(entity.EntityFound, out var age))
                        {
                            missingChild.Age = age;
                        }
                        else
                        {
                            logger.LogWarning($"Unable to parse {entity.EntityFound} as a valid {entity.Type} integer.");
                        }
                        break;
                    case "Gender":
                        missingChild.Gender = entity.EntityFound;
                        break;
                    case "Ethnicity":
                        missingChild.Ethnicity = entity.EntityFound;
                        break;
                    case "MissingSince":
                        try
                        {
                            // attempt to normalize date in ISO format
                            var span = new Chronic.Core.Parser().Parse(entity.EntityFound);
                            if (span?.Start != null)
                            {
                                missingChild.MissingSince = span.Start.Value.ToString("s");
                            }
                        }
                        catch (Exception)
                        {
                            logger.LogWarning($"Unable to parse {entity.EntityFound} as a valid {entity.Type} datetime.");
                        }
                        break;
                    case "Height":
                        missingChild.Height = entity.EntityFound;
                        break;
                    case "Weight":
                        missingChild.Weight = entity.EntityFound;
                        break;
                    case "Found":
                        missingChild.Found = 1;
                        break;
                    default:
                        logger.LogWarning($"No mapping defined for {entity.Type}.");
                        break;
                }
            }
        }
    }
}