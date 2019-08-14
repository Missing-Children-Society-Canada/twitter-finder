using System.Net.Http;
using System.Threading.Tasks;
using MCSC.Classes;
using Newtonsoft.Json;
using System.Net;

namespace MCSC
{
    internal class LuisHelper
    {
        private static readonly string LuisAppID = Utils.GetEnvVariable("LUISappID");
        private static readonly string LuisKey = Utils.GetEnvVariable("LUISsubscriptionKey");
        private static readonly string LuisEndpoint = Utils.GetEnvVariable("LUISendpoint");
        
        ///<summary>
        /// Returns a json string containing the results obtained 
        /// from the LUIS service defined in the env variables
        ///</summary>
        public static async Task<LuisResult> GetLuisResult(string shortSummary)
        {
            var httpClient = GetHttpClient();
            string luisUri = $"{LuisEndpoint}{LuisAppID}?verbose=true&timezoneOffset=-360&q=\"{shortSummary}\"";
            var response = await httpClient.GetAsync(luisUri);

            var contents = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var luisResult = JsonConvert.DeserializeObject<LuisResult>(contents);
                return luisResult;
            }
            return null;
        }

        private static HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", LuisKey);
            return httpClient;
        }
    }
}