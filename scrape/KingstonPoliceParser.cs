using System;
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    internal class KingstonPoliceParser : IBodyParse
    {
        public string Uri => "kingstonpolice.ca";

        public Incident Parse(string body)
        {
            string shortSummary = "";
            string summary = body;
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(body);
                var infoNodes = document.DocumentNode.SelectNodes("//div[@class='iCreateDynaToken']");
                if (infoNodes != null)
                {
                    // Pull only the inner HTML
                    foreach (HtmlNode n in infoNodes)
                    {
                        shortSummary = shortSummary + n.InnerText;
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
            return new Incident
            {
                ShortSummary = shortSummary,
                Summary = summary
            };
        }
    }
}