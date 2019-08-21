using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class BcRcmpScraper : IScraper
    {
        // Example: http://bc.rcmp-grc.gc.ca/ViewPage.action?siteNodeId=2087&languageId=1&contentId=57000
        public Incident Scrape(string body)
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
                    if (!string.IsNullOrEmpty(shortSummary))
                    {
                        // Got something meaningful from short summary!
                        summary = shortSummary;
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Proper Error handling!!!
                summary = body;
            }

            // return the results
            return new Incident(shortSummary, summary);
        }
    }
}