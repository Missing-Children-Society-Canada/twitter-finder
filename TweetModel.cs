using System;

namespace MCSC
{
    /// <summary>
    /// Represents a tweet post.
    /// </summary>
    public class TweetModel
    {
        /// <summary>
        /// Text content of the tweet
        /// </summary>
        public string TweetText { get; set; }

        /// <summary>
        /// Id of the tweet
        /// </summary>
        public string TweetId { get; set; }

        /// <summary>
        /// Time at which the tweet was posted
        /// </summary>
        public DateTime CreatedAtIso { get; set; }
        
        /// <summary>
        /// Name of the user who has posted the tweet
        /// </summary>
        public string TweetedBy { get; set; }
        
        /// <summary>
        /// Represents an original tweet post.
        /// </summary>
        public TweetModel OriginalTweet { get; set; }
    }
}