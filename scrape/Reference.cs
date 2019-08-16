using System;
using HtmlAgilityPack;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MCSC.Classes;
using Microsoft.Extensions.Logging;

namespace MCSC.Parsing
{
    public class Reference
    {
        private readonly string _uri;
        private readonly ILogger _logger;

        public Reference(string uri, ILogger logger)
        {
            _uri = uri;
            _logger = logger;
        }

        private string Body()
        {
            try
            {
                var web = new AutoDecompressWebClient();
                var data = web.DownloadString(this._uri);
                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                // The body is where the majority of the content is so use that
                return doc.DocumentNode
                    .SelectNodes("//body")
                    .First()
                    .InnerHtml;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error extracting body from external reference document.");
                return string.Empty;
            }
        }

        public Incident Load()
        {
            var body = this.Body();
            if (String.IsNullOrEmpty(body))
            {
                _logger.LogInformation("External reference body is empty.");
                return new Incident();
            }

            string cutdownbody = "";
            try
            {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);

                // The script and Style tags are never useful so remove them before more specific processing
                document.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style")
                    .ToList()
                    .ForEach(n => n.Remove());

                // Cutdown body is the body just without Style and script tags
                cutdownbody = document.DocumentNode.InnerHtml;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error cutting script and style tags from external reference document.");
                cutdownbody = body;
            }

            Incident incident;
            var parser = ParserFactory.Instance.BuildParser(this._uri);
            if (parser == null)
            {
                _logger.LogWarning($"No specific parser defined for site @ {this._uri}, using fallback.");
                incident = new Incident()
                {
                    // We don't want to set the short summary because then luis will parse it!
                    // ShortSummary = body,
                    Summary = body
                };
            }
            else
            {
                incident = parser.Parse(cutdownbody);
            }

            var shortSummary = "";
            var summary = "";
            if (!String.IsNullOrEmpty(incident.ShortSummary))
            {
                // If the short summary is available then cut it down to be optimized for LUIS
                shortSummary = this.ShortSummaryCleanUp(incident.ShortSummary);
            }

            if (!String.IsNullOrEmpty(incident.Summary))
            {
                // If the summary is available then cut it down to make it more human readable
                summary = this.SummaryCleanUp(incident.Summary);
            }

            return new Incident
            {
                ShortSummary = shortSummary,
                Summary = summary
            };
        }

        // Clean up for the summary
        // The summary is the human readable content that is displayed in the ESRI portal 
        public string SummaryCleanUp(string body)
        {
            //Console.WriteLine("Inside Clean up!!! \n");
            // Remove all tags
            string result = Regex.Replace(body, "<.*?>", " ");

            result = WebUtility.HtmlDecode(result);

            result = Regex.Replace(result, @"[#]+|\\s+|\t|\n|\r", " ");

            // Remove all extra spaces
            var options = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant;
            Regex regex = new Regex("[ ]{2,}", options);
            result = regex.Replace(result, " ");

            result = result.Trim();
            return result;
        }

        // Clean up for the ShortSummary
        // The short summary is the shortened version of the summary that is optimized to be processed by LUIS
        public string ShortSummaryCleanUp(string body)
        {
            // Remove all tags
            string result = Regex.Replace(body, "<.*?>", " ");
            result = result.ToLower();
            result = result.Replace("&nbsp;", " ");
            result = result.Replace("&quot;", " ");
            result = result.Replace("&ndash;", " ");
            result = result.Replace("&rsquo;", " ");
            result = result.Replace("&#8217;", "'");
            result = result.Replace("&#8243;", "\"");

            string[] FillerWords = {"a", "and", "the", "on","or", "at", "was", "with", "contact", "nearest", "detachment",
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
             "danger", "described", "vunerable", "picture", "friend", "thinks", "things", "media", "year", "about", "providers", "cash", "unsuccessful", "attempts", 
             "accurately", "slimmer", "slightly", "however", "nevertheless", "nike", "adidas", "puma", "joggers"};

            result = Regex.Replace(result, @"\b" + string.Join("\\b|\\b", FillerWords) + "\\b", "");
            result = Regex.Replace(result, @"[&.;()@#~_]+|\\s+|\t|\n|\r", " ");

            // Remove all extra spaces
            var options = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant;
            Regex regex = new Regex("[ ]{2,}", options);
            result = regex.Replace(result, " ");

            return result.Trim();
        }
    }
}