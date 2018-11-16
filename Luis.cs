using System.Net.Http;
using System.Threading.Tasks;
using MCSC.Classes;
using Newtonsoft.Json;
using System.Net;
namespace MCSC
{
    class LUISHelper
    {
        static string LuisAppID = Utils.GetEnvVariable("LUISappID");
        static string LuisKey= Utils.GetEnvVariable("LUISsubscriptionKey");
        static string LuisEndpoint = Utils.GetEnvVariable("LUISendpoint");
        

        ///<summary>
        /// Returns a json string containing the results obtained 
        /// from the LUIS service defined in the env variables
        ///</summary>
        public async Task<MissingChild> GetLuisResult(string shortSummary, MissingChild missingChild)
        {
            var httpClient = GetHttpClient();
            string luisUri = LuisEndpoint + LuisAppID + "/?verbose=true&timezoneOffset=-360&q=";
            string connectionString = $"{luisUri}\"{shortSummary}\"";
            var response = await httpClient.GetAsync(connectionString);

            var contents = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var luisResult = JsonConvert.DeserializeObject<LuisResult>(contents);
                foreach(var entity in luisResult.Entities)
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
            return missingChild;
        }

        static HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", LuisKey);
            return httpClient;
        }
    }
}