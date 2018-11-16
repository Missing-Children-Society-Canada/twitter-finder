using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class ReginaPoliceParser : IBodyParse
    {
        public string Uri
        {
            get {
                return "reginapolice.ca";
            }
        }
        public Incident Parse(String body){
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
                    foreach (HtmlAgilityPack.HtmlNode node in nodes)
                        shortSummary = shortSummary + node.InnerText;
                }
                nodes = document.DocumentNode.SelectNodes("//div[@id='content']/p/text()");
                if (nodes != null && nodes.Count > 0)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in nodes)
                        shortSummary = shortSummary + node.InnerHtml;
                }
               nodes = document.DocumentNode.SelectNodes("//div[@class='entry-content']/p/text()");
                if (nodes != null && nodes.Count > 0 )
                {
                    foreach (HtmlAgilityPack.HtmlNode node in nodes)
                        shortSummary = shortSummary + node.InnerHtml;
                }
                if (!string.IsNullOrEmpty(shortSummary))
                {
                    // Got something meaningful from short summary! 
                    // Override the summary with the more condensed short summary
                    summary = shortSummary;
                }
            }
            catch (Exception e)
            {
                //TODO: appropriate error logging
                summary = body;
            }
            return new Incident()
            {
                ShortSummary = shortSummary,
                Summary = summary
            };

            
        }
    }
}

