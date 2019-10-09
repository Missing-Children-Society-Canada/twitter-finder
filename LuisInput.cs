namespace MCSC
{
    public class LuisInput
    {
        /// <summary>
        /// The short summary is the summary condensed to be parsed by LUIS
        /// If parsing failed, or there was no parser written Short Summary may be null or empty
        /// </summary>
        public string ShortSummary { get; set; }

        /// <summary>
        /// The summary is what was pulled from the Body of the webpage
        /// May be the entire body, may be a more speciifc part of the page or may be null or empty if there was an error
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The URL of the source page when the HTML body was pulled from
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Link of the original tweet
        /// </summary>
        public string TweetUrl { get; set; }

        /// <summary>
        /// Link of the Twitter profile that tweeted the tweet
        /// </summary>
        public string TwitterProfileUrl { get; set; }
        
        /// <summary>
        /// Location of the user that posted the tweet
        /// </summary>
        public string UserLocation { get; set; }
    }
}