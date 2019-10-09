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
        
        /// <summary>
        ///  Link that we scraped (could be null if there was nothing to scrape)
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        ///  Full scraped summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Link of the original tweet
        /// </summary>
        public string TweetUrl { get; set; }

        /// <summary>
        /// Link of the Twitter profile that tweeted the tweet
        /// </summary>
        public string TwitterProfileUrl { get; set; }

        /// <summary>
        /// Short version of the scraped summary, useful for LUIS processing
        /// </summary>
        public string ShortSummary { get; set; }

        /// <summary>
        /// Location of the user that made the tweet
        /// </summary>
        public string UserLocation { get; set; }
    }
}