using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class SaskatoonPoliceParser : IBodyParse
    {
        // Example: http://saskatoonpolice.ca/news/2018795
        public string Uri
        {
            get
            {
                return "saskatoonpolice.ca";
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

                var node = document.DocumentNode.SelectNodes("//section[@class='newsbody']").First();
                if (node != null)
                {
                    summary = node.InnerHtml;
                    //Remove everything but the <p> that's where all the info is!
                    foreach (var d in  node.Descendants().ToList())
                    {
                        if (d.Name == "p")
                        {
                            shortSummary = shortSummary + d.InnerHtml;
                        }
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