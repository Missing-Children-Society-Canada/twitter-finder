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
    }
}