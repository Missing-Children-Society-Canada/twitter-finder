using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class NiagaraPoliceScraper : IScraper
    {
        // Example: https://www.niagarapolice.ca/en/news/index.aspx?newsId=9af514bb-6058-419a-bae2-13e2841aa269
        public Incident Scrape(string body)
        {   
            string shortSummary = "";
            string summary = body;
            try 
            {
                var document = new HtmlDocument();
                document.LoadHtml(body);
                var node = document.GetElementbyId("news_content");
                if (node != null)
                {
                    shortSummary = node.InnerHtml;
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