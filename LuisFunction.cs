using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
                ShortSummary = luisInput.ShortSummary,
                UserLocation = luisInput.UserLocation
            };

            string shortSummary = luisInput.ShortSummary;
            if (!string.IsNullOrEmpty(shortSummary))
            {
                string[] sentences = Regex.Split(shortSummary, @"(?<=[\.!\?])\s+");
                List<LuisV2Entity> allEntities = new List<LuisV2Entity>();
                foreach (string batch in BatchSentences(sentences, 500))
                {
                    logger.LogInformation($"Sending LUIS query \"{batch}\".");
                    var luisResult = await GetLuisResult(batch, logger);
                    if (luisResult != null)
                    {
                        if (luisResult.TopScoringIntent.Intent == "GetDescription")
                        {
                            allEntities.AddRange(luisResult.Entities);
                        }
                    }
                    else
                    {
                        logger.LogWarning("LUIS did not return entities to process.");
                    }
                }

                MapLuisResultToMissingPerson(missingPerson, allEntities, logger);
            }
            else
            {
                logger.LogWarning($"LUIS skipped tweet {luisInput.TweetUrl} due to missing short summary.");
            }

            var result = JsonConvert.SerializeObject(missingPerson);
            logger.LogInformation($"result:\n{result}");
            return result;
        }

        private static IEnumerable<string> BatchSentences(string[] sentences, int maxLen)
        {
            for (var index = 0; index < sentences.Length; index++)
            {
                var sentence = sentences[index];
                if (sentence.Length > maxLen)
                {
                    yield return sentence.Substring(0, maxLen);
                }
                else
                {
                    string currentBatch = sentence;
                    int j = index + 1;
                    while (j < sentences.Length)
                    {
                        var nextSentence = sentences[j];
                        if (currentBatch.Length + nextSentence.Length + 1 > maxLen)
                        {
                            break;
                        }
                        currentBatch += " " + nextSentence;
                        index++;
                        j++;
                    }
                    yield return currentBatch;
                }
            }
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

        private static void MapLuisResultToMissingPerson(MissingPerson missingPerson, List<LuisV2Entity> entities, ILogger logger)
        {
            //construct the missing person record from the LUIS result, some properties are constructed from a combination of luis results

            // select the best name from the names that are returned using heuristic
            var nameEntity =
                entities.FirstOrDefault(f => f.Type == "builtin.personName" && f.Entity.Contains(' ') && f.Role == "subject") ??
                entities.FirstOrDefault(f => f.Type == "builtin.personName" && f.Entity.Contains(' ') && f.Role == null) ??
                entities.SelectTopScore("Name");
            string personName = nameEntity?.Entity;
            if (!string.IsNullOrEmpty(personName))
            {
                var myTi = new CultureInfo("en-US", false);
                personName = myTi.TextInfo.ToTitleCase(personName);
            }
            missingPerson.Name = personName;

            // Select the best report location 
            var cityEntity = 
                //entities.FirstOrDefault(f => f.Type == "builtin.geographyV2.city") ??
                entities.SelectTopScore("City");
            missingPerson.City = cityEntity?.Entity;

            missingPerson.Province =
                entities.FirstOrDefault(f => f.Type == "provinceV2")?.Resolution.FirstOrDefaultElement() ??
                entities.SelectTopScore("Province")?.Entity;

            var age = entities.SelectTopScoreInt("Age") ??
                entities.FirstOrDefault(f => f.Type == "builtin.age")?.FirstOrDefaultAgeResolution() ??
                entities.FirstOrDefault(f => f.Type == "ageV2")?.EntityAsInt();
            missingPerson.Age = age.GetValueOrDefault(0);

            missingPerson.Gender = 
                entities.FirstOrDefault(f => f.Type == "genderV2")?.Resolution.FirstOrDefaultElement() ??
                entities.SelectTopScore("Gender")?.Entity;
            
            missingPerson.Ethnicity =
                entities.FirstOrDefault(f => f.Type == "ethnicityV2")?.Resolution.FirstOrDefaultElement() ??
                entities.SelectTopScore("Ethnicity")?.Entity;
            
            missingPerson.MissingSince = entities.SelectTopScoreDateTime("MissingSince");
            
            missingPerson.Height = entities.SelectTopScore("Height")?.Entity;
            missingPerson.Weight = entities.SelectTopScore("Weight")?.Entity;
            
            // if a found entity exists then the result is 
            missingPerson.Found = entities.Exists(w=>w.Type == "Found") ||
                                  string.Equals(entities.FirstOrDefault(f => f.Type == "locatedV2")?.Resolution.FirstOrDefaultElement(), "Located", StringComparison.OrdinalIgnoreCase)
                                  ? 1 : 0;
        }
    }
}