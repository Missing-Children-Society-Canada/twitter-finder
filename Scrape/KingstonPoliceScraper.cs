using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    internal class KingstonPoliceScraper : IScraper
    {
        public Incident Scrape(string body)
        {
            string shortSummary = "";
            string summary = body;
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(body);
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='iCreateDynaToken']");
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
                        // Override the summary with the more condensed short summary
                        summary = shortSummary;
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Log meaningful error
            }
            return new Incident(shortSummary, summary);
        }
    }
}