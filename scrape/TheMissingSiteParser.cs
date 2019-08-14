using System;
using System.Linq;
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class TheMissingParser : IBodyParse
    {
        public string Uri
        {
            get
            {
                return "themissing.ca";
            }
        }
        // Parser for https://www.themissing.ca
        // Example: view-source:https://www.themissing.ca/listing/treasure-spoon-13-missing-girl-from-thunder-bay-ontario/

        public Incident Parse(string body)
        {
            string summary = "";
            string shortSummary = "";
            try
            {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);
                
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='pf-itempage-desc descexpf']/p");
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