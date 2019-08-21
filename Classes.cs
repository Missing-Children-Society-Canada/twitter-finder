using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MCSC
{
    public class LuisInput
    {
        // The short summary is the summary condensed to be parsed by LUIS
        // If parsing failed, or there was no parser written Short Summary may be null or empty
        public string ShortSummary { get; set; }
        // The summary is what was pulled from the Body of the webpage
        // May be the entire body, may be a more speciifc part of the page or may be null or empty if there was an error
        public string Summary { get; set; }
        // The URL of the source page when the HTML body was pulled from
        public string SourceUrl { get; set; }
        // Link of the original tweet
        public string TweetUrl { get; set; }
        // Link of the Twitter profile that tweeted the tweet
        public string TwitterProfileUrl { get; set; }
    }

    public class Incident
    {
        public Incident(string shortSummary, string summary)
        {
            ShortSummary = shortSummary;
            Summary = summary;
        }

        // The short summary is the summary condensed to be parsed by LUIS
        // If parsing failed, or there was no parser written Short Summary may be null or empty
        public string Summary { get; }
        // The summary is what was pulled from the Body of the webpage
        // May be the entire body, may be a more speciifc part of the page or may be null or empty if there was an error
        public string ShortSummary { get; }

        public override string ToString() => $"ShortSummary: {this.ShortSummary}";
    }

    public class Tweet
    {
        public string TweetText { get; set; }
        public string TweetId { get; set; }
        public DateTime CreatedAtIso { get; set; }
        public string TweetedBy { get; set; }
        public string TwitterProfileURL { get; set; }
        public string TweetUrl { get; set; }
        public string SourceUrl { get; set; }
    }

    public class MissingChild
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public string MissingSince { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public int Found { get; set; }
        // Link that we scraped (could be null if there was nothing to scrape)
        public string SourceUrl { get; set; }
        // Full scraped summary
        public string Summary { get; set; }
        // Link of the original tweet
        public string TweetUrl { get; set; }
        // Link of the Twitter profile that tweeted the tweet
        public string TwitterProfileUrl { get; set; }
        // Short version of the scraped summary, useful for LUIS processing
        public string ShortSummary { get; set; }
    }

    public class Entity
    {
        [JsonProperty("entity")]
        public string EntityFound { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("score")]
        public double? Score { get; set; }
    }

    public class LuisResult
    {
        [JsonProperty("entities")]
        public List<Entity> Entities { get; set; }
    }
}