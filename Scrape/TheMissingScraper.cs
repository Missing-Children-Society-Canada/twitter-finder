using System;
using System.Linq;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class TheMissingScraper : IScraper
    {
        // Parser for https://www.themissing.ca
        // Example: view-source:https://www.themissing.ca/listing/treasure-spoon-13-missing-girl-from-thunder-bay-ontario/
        public Incident Scrape(string body)
        {
            string summary = "";
            string shortSummary = "";
            try
            {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);
                
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='pf-itempage-desc descexpf']");
                if (infoNodes != null)
                {
                    foreach (HtmlNode node in infoNodes)
                    {
                        node.Descendants()
                        .Where(n => n.Name == "script" || n.Name == "style")
                        .ToList()
                        .ForEach(n => n.Remove());
                    }

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