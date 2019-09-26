using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCSC
{
    public class LuisV2Result
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("topScoringIntent")]
        public LuisV2Intent TopScoringIntent { get; set; }

        [JsonProperty("entities")]
        public List<LuisV2Entity> Entities { get; set; }
    }

    public class LuisV2Intent
    {
        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }
    }

    public class LuisV2Entity
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("score")]
        public double? Score { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("startIndex")]
        public int? StartIndex { get; set; }

        [JsonProperty("endIndex")]
        public int? EndIndex { get; set; }

        [JsonProperty("resolution")]
        public Dictionary<string, object> Resolution { get; set; }
    }

    public static class LuisV2Extension
    {
        public static int? EntityAsInt(this LuisV2Entity luisV2)
        {
            //extract the integer from the entity text 
            var match = System.Text.RegularExpressions.Regex.Match(luisV2.Entity, @"\d{1,2}");
            if(match.Success)
            {
                var resultString = match.Value;
                if(!string.IsNullOrEmpty(resultString) && int.TryParse(resultString, out var i))
                {
                    return i;
                }
            }
            return null;
        }

        public static int? FirstOrDefaultAgeResolution(this LuisV2Entity luisV2)
        {
            //extract the integer from the entity text 
            if(luisV2.Resolution.ContainsKey("unit") && luisV2.Resolution.ContainsKey("value"))
            {
                var oUnit = luisV2.Resolution["unit"];
                var oValu = luisV2.Resolution["value"];
                if(oUnit.ToString() == "Year" && int.TryParse(oValu.ToString(), out var i ))
                {
                    return i;
                }
            }
            return null;
        }
    }
}