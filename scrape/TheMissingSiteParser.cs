using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
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
            try {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);
                
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='pfdetailitem-subelement pf-onlyitem clearfix']");
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
                        shortSummary = shortSummary + n.InnerHtml;
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
            return new Incident()
            {
                Summary = summary,
                ShortSummary = shortSummary,
            };
        }
    }
}