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
            logger.LogInformation($"LuisFunction invoked:\n{json}");

            var luisInput = JsonConvert.DeserializeObject<LuisInput>(json);

            var missingPerson = new MissingPerson
            {
                SourceUrl = luisInput.SourceUrl,
                TweetUrl = luisInput.TweetUrl,
                TwitterProfileUrl = luisInput.TwitterProfileUrl,
                Summary = luisInput.Summary,
                ShortSummary = luisInput.ShortSummary
            };

            string shortSummary = luisInput.ShortSummary;
            if (!string.IsNullOrEmpty(shortSummary))
            {
                shortSummary = shortSummary.Replace("&","");
                // take only first 500 characters so LUIS can handle it 
                shortSummary = shortSummary.Substring(0, Math.Min(shortSummary.Length, 500));

                logger.LogInformation($"Sending LUIS query \"{shortSummary}\".");

                var luisResult = await GetLuisResult(shortSummary, logger);
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
                logger.LogWarning($"LUIS skipped tweet {luisInput.TweetUrl} due to missing short summary.");
            }

            var result = JsonConvert.SerializeObject(missingPerson);
            logger.LogInformation($"result:\n{result}");
            return result;
        }

        ///<summary>
        /// Returns a json string containing the results obtained 
        /// from the LUIS service defined in the env variables
        ///</summary>
        private static async Task<LuisV2Result> GetLuisResult(string shortSummary, ILogger logger)
        {
            string luisAppID = Environment.GetEnvironmentVariable("LUISappID", EnvironmentVariableTarget.Process);
            string luisEndpoint = Environment.GetEnvironmentVariable("LUISendpoint", EnvironmentVariableTarget.Process);
            string luisStaging = Environment.GetEnvironmentVariable("LUISstaging", EnvironmentVariableTarget.Process);

            string luisUri = $"{luisEndpoint}{luisAppID}?verbose=false{luisStaging}&timezoneOffset=-360&q={HttpUtility.UrlEncode(shortSummary)}";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("LUISsubscriptionKey", EnvironmentVariableTarget.Process));
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
                luisResult.Entities.FirstOrDefault(f => f.Type == "provinceV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Province")?.Entity;

            var age = luisResult.Entities.SelectTopScoreInt("Age") ??
                luisResult.Entities.FirstOrDefault(f => f.Type == "builtin.age")?.FirstOrDefaultAgeResolution() ??
                luisResult.Entities.FirstOrDefault(f => f.Type == "ageV2")?.EntityAsInt();
            missingPerson.Age = age.GetValueOrDefault(0);

            missingPerson.Gender = 
                luisResult.Entities.FirstOrDefault(f => f.Type == "genderV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Gender")?.Entity;
            
            missingPerson.Ethnicity =
                luisResult.Entities.FirstOrDefault(f => f.Type == "ethnicityV2")?.Resolution.FirstOrDefaultElement() ??
                luisResult.Entities.SelectTopScore("Ethnicity")?.Entity;
            
            missingPerson.MissingSince = luisResult.Entities.SelectTopScoreDateTime("MissingSince");
            
            missingPerson.Height = luisResult.Entities.SelectTopScore("Height")?.Entity;
            missingPerson.Weight = luisResult.Entities.SelectTopScore("Weight")?.Entity;
            
            // if a found entity exists then the result is 
            missingPerson.Found = luisResult.Entities.Exists(w=>w.Type == "Found") ? 1 : 0;
        }
    }
}