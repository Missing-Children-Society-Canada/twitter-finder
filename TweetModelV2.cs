using System;
using System.Collections.Generic;
using System.Linq;

namespace MCSC.V2
{
    public class ContainerModel
    {
        public List<TweetModel> Data { get; set; }
        public IncludesModel Includes { get; set; }

        public List<MCSC.TweetModel> ConvertToArchived()
        {
            List<MCSC.TweetModel> tweets = new List<MCSC.TweetModel>();

            foreach (var tweet in Data)
            {
                // Get the user for the tweet
                var user = Includes.Users.FirstOrDefault(w => w.Id == tweet.Author_Id);

                var archivedTweet = new MCSC.TweetModel
                {
                    TweetId = tweet.Id,
                    TweetText = tweet.Text,
                    CreatedAtIso = tweet.Created_At,
                    TweetedBy = user?.Username,
                    UserDetails = user != null ?
                        new MCSC.UserDetailsModel
                        {
                            Name = user.Name,
                            Location = user.Location
                        } :
                        null
                };

                tweets.Add(archivedTweet);
            }

            return tweets;
        }
    }

    /// <summary>
    /// Represents a tweet post.
    /// </summary>
    public class TweetModel
    {
        /// <summary>
        /// Text content of the tweet
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Id of the tweet
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Time at which the tweet was posted
        /// </summary>
        public DateTime Created_At { get; set; }
        
        /// <summary>
        /// The ID of the user that authored the tweet
        /// </summary>
        public string Author_Id { get; set; }
    }

    public class IncludesModel
    {
        public List<UserModel> Users { get; set; }
    }

    public class UserModel
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Location of the user
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// User's username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///  Description of the user account
        /// </summary>
        public string Description { get; set; }


    }
}