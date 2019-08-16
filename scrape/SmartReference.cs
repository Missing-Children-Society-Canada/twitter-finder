using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace MCSC.Parsing
{
    public class SmartReference
    {
        private readonly string _uri;
        private readonly ILogger _logger;

        public SmartReference(string uri, ILogger logger)
        {
            this._uri = uri;
            this._logger = logger;
        }

        public bool Load(out string shortSummary, out string summary)
        {
            try
            {
                var web = new AutoDecompressWebClient();
                var data = web.DownloadString(_uri);

                data = ReplaceSomeEncodings(data);

                var sr = new SmartReader.Reader(_uri, data);
                var article = sr.GetArticle();
                if (!string.IsNullOrEmpty(article.TextContent))
                {
                    shortSummary = ShortSummaryCleanUp(article.TextContent);
                    summary = SummaryCleanUp(article.TextContent);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception loading article");
            }

            summary = null;
            shortSummary = null;
            return false;
        }

        private static string ReplaceSomeEncodings(string input)
        {
            return input
                .Replace("&nbsp;", " ")
                .Replace("&quot;", "\"")
                .Replace("&ndash;", "-")
                .Replace("&rsquo;", "'")
                .Replace("&lsquo;", "'")
                .Replace("&#8217;", "'")
                .Replace("&#8243;", "\"");
        }

        // Clean up for the summary
        // The summary is the human readable content that is displayed in the ESRI portal 
        private static string SummaryCleanUp(string body)
        {
            string result = Regex.Replace(body, @"[#]+|\\s+|\t|\n|\r", " ");

            // Remove all extra spaces
            Regex regex = new Regex("[ ]{2,}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            result = regex.Replace(result, " ");
            return result.Trim();
        }

        // Clean up for the ShortSummary
        // The short summary is the shortened version of the summary that is optimized to be processed by LUIS
        private static string ShortSummaryCleanUp(string body)
        {
            string result = body.ToLower();

            string[] fillerWords = {"a", "and", "the", "on","or", "at", "was", "with", "contact", "nearest", "detachment",
             "failed", "investigations", "anyone", "regarding", "approximately", "in", "is", "uknown", "time", "of", "any", "to", "have", "seen",
             "if", "UnknownClothing", "applicable", "UnknownFile", "it", "unknownclothing", "information","unknownfile", "police", "service", "call", "crime",
             "stoppers", "from", "by", "all", "also", "that", "his", "please", "been", "this", "concern", "they","are","they","as","had","wearing",
             "color", "colour", "shirt", "pants","be", "believed", "guardians", "network", "coordinated", "response", "without","complexion",
             "has", "for", "well-being", "there", "included", "release", "picture", "family", "younger", "shorts", "described", "reported", "police", "officer",
             "public", "attention", "asked", "live", "own", "complexity", "victimize", "children", "child", "nations", "when", "person", "jeans", "shoes", "thin",
             "area", "road", "criminal", "investigation", "division", "concerned", "concern", "build", "assistance", "seeking", "locate", "locating", "stripe",
             "stripes", "straight", "requesting", "request", "requests", "facebook", "twitter", "avenue", "road", "street", "large", "long", "tiny", "hoodie",
             "leggings", "sweater", "jacket", "boots", "tennis shoes", "leather", "worried", "backpack", "purse", "whereabouts", "unknown", "help",
             "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday", "block", "crossing", "harm", "not",
             "danger", "described", "vunerable", "picture", "friend", "thinks", "things", "media", "about", "providers", "cash", "unsuccessful", "attempts",
             "accurately", "slimmer", "slightly", "however", "nevertheless", "nike", "adidas", "puma", "joggers"};

            result = Regex.Replace(result, @"\b" + string.Join("\\b|\\b", fillerWords) + "\\b", "");
            result = Regex.Replace(result, @"[&.;()@#~_]+|\\s+|\t|\n|\r", " ");

            // Remove all extra spaces
            Regex regex = new Regex("[ ]{2,}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            result = regex.Replace(result, " ");
            return result.Trim();
        }
    }
}