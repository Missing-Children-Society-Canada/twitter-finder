using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Web;

namespace MCSC
{
    public static class LuisFunction
    {
        [StorageAccount("BlobStorageConnectionString")]
        [return: Queue("luis")]
        [FunctionName("LuisFunction")]
        public static async Task<string> Run([QueueTrigger("scrape")]string json, 
            ILogger logger)
        {
            logger.LogInformation($"Luis function invoked: {json}");

            var luisInput = JsonConvert.DeserializeObject<LuisInput>(json);

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("LUISsubscriptionKey", EnvironmentVariableTarget.Process));

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
                shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 500));

                logger.LogInformation($"Sending LUIS query \"{shortSummary}\".");

                var luisResult = await GetLuisResult(httpClient, shortSummary, logger);
                if (luisResult != null)
                {
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

            return JsonConvert.SerializeObject(missingPerson);
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

            string luisUri = $"{luisEndpoint}{luisAppID}?verbose=false{luisStaging}&timezoneOffset=-360&q={HttpUtility.UrlEncode(shortSummary)}";
            
            var response = await httpClient.GetAsync(luisUri);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var contents = await response.Content.ReadAsStringAsync();
                logger.LogInformation($"LUIS returned: {contents}.");
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
                luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.personName" && f.Entity.Contains(' ') && f.Role == "subject") ??
                luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.personName" && f.Entity.Contains(' ') && f.Role == null) ??
                luisResult.Entities.SelectTopScore("Name");
            missingPerson.Name = nameEntity?.Entity;

            // Select the best report location 
            var cityEntity = 
                //luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.geographyV2.city") ??
                luisResult.Entities.SelectTopScore("City");
            missingPerson.City = cityEntity?.Entity;

            missingPerson.Province =
                luisResult.Entities.SelectTopScore("provinceV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Province")?.Entity;

            missingPerson.Age = luisResult.Entities.SelectTopScoreInt("Age").GetValueOrDefault(0);

            missingPerson.Gender = 
                luisResult.Entities.SelectTopScore("genderV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Gender")?.Entity;
            
            missingPerson.Ethnicity =
                luisResult.Entities.SelectTopScore("ethnicityV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Ethnicity")?.Entity;
            
            var missingSinceEntity = luisResult.Entities.SelectTopScoreDateTime("MissingSince");
            missingPerson.MissingSince = missingSinceEntity?.ToString("s");
            
            missingPerson.Height = luisResult.Entities.SelectTopScore("Height")?.Entity;
            missingPerson.Weight = luisResult.Entities.SelectTopScore("Weight")?.Entity;
            
            // if a found entity exists then the result is 
            missingPerson.Found = luisResult.Entities.Exists(w=>w.Type == "Found") ? 1 : 0;
        }
    }
}