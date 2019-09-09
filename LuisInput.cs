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
}