using System;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class BrandonPoliceScraper : IScraper
    {
        public Incident Scrape(string body)
        {
             // Load the document
            var document = new HtmlDocument();
            document.LoadHtml(body);
            var summary = "";
            var shortSum = "";
            try
            {
                var nodes = document.DocumentNode.SelectNodes("//article[@class='item-pagemediaReleases']/p/span/text()");
                if (nodes != null)
                {
                    foreach (HtmlNode node2 in nodes)
                    {
                        shortSum = shortSum + node2.InnerHtml;
                    }
                    summary = shortSum;
                }

                nodes = document.DocumentNode.SelectNodes("//article[@class='item-pagemediaReleases']/p/text()");
                if (nodes != null)
                {
                    foreach (HtmlNode node2 in nodes)
                    {
                        shortSum = shortSum + node2.InnerHtml;
                    }
                    summary = summary + shortSum;   
                }
            }
            catch(Exception)
            {
                //TODO: log could not parse page. 
                summary = body;
            }

            return new Incident(shortSum, summary);
        }
    }
}