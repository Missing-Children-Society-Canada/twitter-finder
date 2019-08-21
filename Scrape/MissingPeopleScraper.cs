using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class MissingPeopleScraper : IScraper
    {
        // Example: http://missingpeople.ca/2018/11/missing-girl-in-iqaluit-nunavut-alison-bracken-12/
        public Incident Scrape(string body)
        {   
            string shortSummary = "";
            string summary = body;
            try 
            {
                var document = new HtmlDocument();
                document.LoadHtml(body);
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='story']");
                if (infoNodes != null)
                {
                    // Pull only the inner HTML
                    foreach (HtmlNode n in infoNodes)
                    {
                        shortSummary = shortSummary + n.InnerHtml;
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