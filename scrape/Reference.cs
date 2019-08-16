using System;
using HtmlAgilityPack;
using System.Linq;
using System.Net;
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
        private string SummaryCleanUp(string body)
        {
            string result = StringSanitizer.RemoveHtmlTags(body);
            result = WebUtility.HtmlDecode(result);
            result = StringSanitizer.RemoveHashtags(result);
            result = StringSanitizer.RemoveDoublespaces(result);
            result = result.Trim();
            return result;
        }

        // Clean up for the ShortSummary
        // The short summary is the shortened version of the summary that is optimized to be processed by LUIS
        private string ShortSummaryCleanUp(string body)
        {
            string result = StringSanitizer.RemoveHtmlTags(body);
            result = StringSanitizer.SimplifyHtmlEncoded(result);
            result = StringSanitizer.RemoveFillerWords(result);
            result = StringSanitizer.RemoveSpecialCharacters(result);
            result = StringSanitizer.RemoveDoublespaces(result);
            return result.Trim();
        }
    }
}