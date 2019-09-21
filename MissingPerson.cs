using System;

namespace MCSC
{
    public class MissingPerson
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Ethnicity { get; set; }
        public DateTime? MissingSince { get; set; }
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
}