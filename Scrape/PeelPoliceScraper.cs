using System;
using System.Linq;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class PeelPoliceScraper : IScraper
    {
        // Example: https://www.peelpolice.ca/Modules/News/index.aspx?feedId=d6aa0ab4-eb5f-4b5e-a251-0e833d984d68&page=2&newsId=9a8a522b-de30-4ab3-9b23-117547215dfe
        public Incident Scrape(string body)
        {
            var document = new HtmlDocument();
            document.LoadHtml(body);
            var shortSummary = "";
            var summary = body;
            try
            {
                var nodes = document.DocumentNode.SelectNodes("//div[@id='news_content']/div/p/text()");
                if (nodes != null && nodes.Count > 0)
                {
                    shortSummary = string.Join(" ", nodes.Take(4).Select(n => n.InnerHtml));
                    if (!string.IsNullOrEmpty(shortSummary))
                    {
                        // Got something meaningful from short summary! 
                        // Override the summary with the more condensed short summary
                        summary = shortSummary;   
                    }
                }
            }
            catch(Exception)
            {
                // TODO : log meanigful error
            }
            return new Incident(shortSummary, summary);
        }
    }
}