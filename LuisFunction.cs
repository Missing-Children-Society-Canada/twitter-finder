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
        public static async Task<string> Run([QueueTrigger("scrape")]string json, ILogger logger)
        {
            var luisInputs = JsonConvert.DeserializeObject<List<LuisInput>>(json);
            logger.LogInformation($"Luis function {luisInputs.Count} items, invoked: {json}");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("LUISsubscriptionKey", EnvironmentVariableTarget.Process));

            IList<MissingPerson> results = new List<MissingPerson>();
            foreach (var luisInput in luisInputs)
            {
                string shortSummary = luisInput.ShortSummary;
                
                var missingPerson = new MissingPerson
                {
                    SourceUrl = luisInput.SourceUrl,
                    TweetUrl = luisInput.TweetUrl,
                    TwitterProfileUrl = luisInput.TwitterProfileUrl,
                    Summary = luisInput.Summary,
                    ShortSummary = luisInput.ShortSummary
                };

                if (!string.IsNullOrEmpty(shortSummary))
                {
                    shortSummary = shortSummary.Replace("&","");
                    // take only first 500 characters so LUIS can handle it 
                    shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 498));

                    logger.LogInformation($"Sending LUIS query \"{shortSummary}\".");

                    var luisResult = await GetLuisResult(httpClient, shortSummary, logger);
                    if (luisResult != null)
                    {
                        var entityKeys = string.Join(",", luisResult.Entities.Select(s => s.Type + "=" + s.Entity));
                        logger.LogInformation($"LUIS returned the following entities:{entityKeys}");

                        if(luisResult.TopScoringIntent.Intent == "GetDescription")
                        {
                            MapLuisResultToMissingPerson(missingPerson, luisResult, logger);
                        }
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

                results.Add(missingPerson);

                await Task.Delay(200);
            }

            return JsonConvert.SerializeObject(results);
        }

        ///<summary>
        /// Returns a json string containing the results obtained 
        /// from the LUIS service defined in the env variables
        ///</summary>
        private static async Task<LuisV2Result> GetLuisResult(HttpClient httpClient, string shortSummary, ILogger logger)
        {
            string luisAppID = Environment.GetEnvironmentVariable("LUISappID", EnvironmentVariableTarget.Process);
            string luisEndpoint = Environment.GetEnvironmentVariable("LUISendpoint", EnvironmentVariableTarget.Process);
            string luisStaging = Environment.GetEnvironmentVariable("LUISstaging", EnvironmentVariableTarget.Process);

            string luisUri = $"{luisEndpoint}{luisAppID}?verbose=false{luisStaging}&timezoneOffset=-360&q=\"{shortSummary}\"";
            
            var response = await httpClient.GetAsync(luisUri);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var contents = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LuisV2Result>(contents);
            }
            logger.LogError($"LUIS returned error status code {response.StatusCode}.");
            return null;
        }

        private static void MapLuisResultToMissingPerson(MissingPerson missingPerson, LuisV2Result luisResult, ILogger logger)
        {
            //construct the missing person record from the LUIS result, some properties are constructed from a combination of luis results

            // select the best name from the names that are returned using heuristic
            var nameEntity =
                luisResult.Entities.FirstOrDefault(f=>f.Type == "builtin.personName" && f.Entity.Contains(' ') && f.Role == "subject") ??
                luisResult.Entities.FirstOrDefault(f=>f.Type == "builtin.personName" && f.Entity.Contains(' ')) ??
                luisResult.Entities.SelectTopScore("Name");
            missingPerson.Name = nameEntity?.Entity;

            // Select the best report location 
            var cityEntity = 
                luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.geographyV2.city") ??
                luisResult.Entities.SelectTopScore("City");
            missingPerson.City = cityEntity?.Entity;

            var provinceEntity = 
                luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.geographyV2.state") ??
                luisResult.Entities.SelectTopScore("Province");
            missingPerson.Province = provinceEntity?.Entity;

            missingPerson.Age = luisResult.Entities.SelectTopScoreInt("Age").GetValueOrDefault(0);

            missingPerson.Gender = luisResult.Entities.SelectTopScore("Gender")?.Entity;
            
            missingPerson.Ethnicity = luisResult.Entities.SelectTopScore("Ethnicity")?.Entity;
            
            var missingSinceEntity = luisResult.Entities.SelectTopScoreDateTime("MissingSince");
            missingPerson.MissingSince = missingSinceEntity?.ToString("s");
            
            missingPerson.Height = luisResult.Entities.SelectTopScore("Height")?.Entity;
            missingPerson.Weight = luisResult.Entities.SelectTopScore("Weight")?.Entity;
            
            // if a found entity exists then the result is 
            missingPerson.Found = luisResult.Entities.Exists(w=>w.Type == "Found") ? 1 : 0;
        }
    }

    internal static class LuisEntityExtensions
    {
        public static LuisV2Entity SelectTopScore(this IEnumerable<LuisV2Entity> entities, string type)
        {
            LuisV2Entity result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                if(entity.Score > topScore || topScore == null)
                {
                    topScore = entity.Score;
                    result = entity;
                }
            }
            return result;
        }

        public static int? SelectTopScoreInt(this IEnumerable<LuisV2Entity> entities, string type)
        {
            int? result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                if(int.TryParse(entity.Entity, out var temp) && (entity.Score > topScore || topScore == null))
                {
                    topScore = entity.Score;
                    result = temp;
                }
            }
            return result;
        }

        public static DateTime? SelectTopScoreDateTime(this IEnumerable<LuisV2Entity> entities, string type)
        {
            DateTime? result = null;
            double? topScore = null;
            foreach(var entity in entities.Where(item => item.Type == type))
            {
                DateTime? temp = null;
                try
                {
                    var span = new Chronic.Core.Parser().Parse(entity.Entity);
                    temp = span.Start;
                }
                catch(Exception)
                {
                    temp = null;
                }
                
                if(temp != null && (entity.Score > topScore || topScore == null))
                {
                    topScore = entity.Score;
                    result = temp;
                }
            }
            return result;
        }
    }
}