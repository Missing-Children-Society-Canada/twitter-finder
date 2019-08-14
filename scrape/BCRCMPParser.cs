using System;
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class BCRCMPParser : IBodyParse
    {
        // Example: http://bc.rcmp-grc.gc.ca/ViewPage.action?siteNodeId=2087&languageId=1&contentId=57000
        public string Uri
        {
            get
            {
                // TODO: Test and fix
                // This website was doing a redirect that was messing up the parsing
                return "bc.rcmp-grc.gc.ca";
            }
        }

        public Incident Parse(string body)
        {
            string summary = "";
            string shortSummary = "";
            try
            {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);

                var infoNodes = document.DocumentNode.SelectNodes("//main");

                if (infoNodes != null)
                {
                    // Pull only the inner HTML
                    foreach (HtmlNode n in infoNodes)
                    {
                        shortSummary = shortSummary + n.InnerText;
                    }
                    if (!String.IsNullOrEmpty(shortSummary))
                    {
                        // Got something meaningful from short summary!
                        summary = shortSummary;
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Proper Error handling!!!
                summary = body;
            }

            // return the results
            return new Incident
            {
                Summary = summary,
                ShortSummary = shortSummary,
            };
        }
    }
}