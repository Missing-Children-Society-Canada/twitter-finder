using System;

namespace MCSC
{
    public class Tweet
    {
        public string TweetText { get; set; }
        public string TweetId { get; set; }
        public DateTime CreatedAtIso { get; set; }
        public string TweetedBy { get; set; }
        public string TwitterProfileURL { get; set; }
        public string TweetUrl { get; set; }
        public string SourceUrl { get; set; }
        public Tweet OriginalTweet { get; set; }
    }
}