using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class ReginaPoliceScraper : IScraper
    {
        public Incident Scrape(string body)
        {
            // Load the document
            var document = new HtmlDocument();
            document.LoadHtml(body);
            var shortSummary = "";
            var summary = body;

            try 
            {
                // TODO: Test with entry-content instead
                var nodes = document.DocumentNode.SelectNodes("//div[@id='content']/ul/li/text()");
                 if (nodes != null && nodes.Count > 0)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        shortSummary = shortSummary + node.InnerText;
                    }
                }
                nodes = document.DocumentNode.SelectNodes("//div[@id='content']/p/text()");
                if (nodes != null && nodes.Count > 0)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        shortSummary = shortSummary + node.InnerHtml;
                    }
                }
               nodes = document.DocumentNode.SelectNodes("//div[@class='entry-content']/p/text()");
                if (nodes != null && nodes.Count > 0 )
                {
                    foreach (HtmlNode node in nodes)
                    {
                        shortSummary = shortSummary + node.InnerHtml;
                    }
                }
                if (!string.IsNullOrEmpty(shortSummary))
                {
                    // Got something meaningful from short summary! 
                    // Override the summary with the more condensed short summary
                    summary = shortSummary;
                }
            }
            catch (Exception)
            {
                //TODO: appropriate error logging
                summary = body;
            }
            return new Incident(shortSummary, summary);
        }
    }
}

