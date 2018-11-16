using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class MissingPeopleParser : IBodyParse
    {
        // Example: http://missingpeople.ca/2018/11/missing-girl-in-iqaluit-nunavut-alison-bracken-12/
        public string Uri
        {
            get
            {
                return "missingpeople.ca";
            }
        }

        public Incident Parse(string body)
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
                    if (!String.IsNullOrEmpty(shortSummary))
                    {
                        // Got something meaningful from short summary! 
                        // Override the summary with the more condensed short summary
                        summary = shortSummary;   
                    }
                }

            }
            catch (Exception e)
            {
                // TODO: Log meaningful error
            }
            return new Incident()
            {
                ShortSummary = shortSummary,
                Summary = summary
            };
        }
    }
}