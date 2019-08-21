using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class RcmpScraper : IScraper
    {
        // Parser for https://www.services.rcmp-grc.gc.ca
        // Example: view-source:https://www.services.rcmp-grc.gc.ca/missing-disparus/case-dossier.jsf?case=2001008160&id=0

        public Incident Scrape(string body)
        {
            string summary = body;
            string shortSummary = "";
            try
            {
                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);

                // List of tags that have most of the content
                var acceptableTags = new [] { "strong", "dt", "dd", "p"};
                var main = document.DocumentNode.SelectNodes("//main");
                var nodes = new Queue<HtmlNode>(main.First().ChildNodes);
                while(nodes.Count > 0)
                {
                    var node = nodes.Dequeue();
                    var parentNode = node.ParentNode;

                    if(!acceptableTags.Contains(node.Name) && node.Name != "#text")
                    {
                        var childNodes = node.SelectNodes("./*|./text()");

                        if (childNodes != null)
                        {
                            // If the node is the right type then add the children to the list to check
                            foreach (var child in childNodes)
                            {
                                nodes.Enqueue(child);
                                parentNode.InsertBefore(child, node);
                            }
                        }
                        // If the node doesn't have the right tags remove it
                        parentNode.RemoveChild(node);
                    }
                }
                // Pull only the inner HTML
                shortSummary = document.DocumentNode.SelectNodes("//main").First().InnerHtml;
            
                // Just drop everything after "Verify current information" as it is just tip info
                int indexOfTipInfo = shortSummary.IndexOf("Verify current information", StringComparison.OrdinalIgnoreCase);
                if(indexOfTipInfo >= 0)
                    shortSummary = shortSummary.Remove(indexOfTipInfo);

                if (!string.IsNullOrEmpty(shortSummary))
                {
                    // Got something meaningful from short summary! 
                    // Override the summary with the more condensed short summary
                    summary = shortSummary;   
                }
            }
            catch(Exception)
            {
                // TO DO: Log meaningful error
            }
            // return the results
            return new Incident(shortSummary, summary);
        }
    }
}