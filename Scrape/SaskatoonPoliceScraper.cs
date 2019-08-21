using System;
using System.Linq;
using HtmlAgilityPack;

namespace MCSC.Scrape
{
    public class SaskatoonPoliceScraper : IScraper
    {
        // Example: http://saskatoonpolice.ca/news/2018795
        public Incident Scrape(string body)
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
                    if (!string.IsNullOrEmpty(shortSummary))
                    {
                        // Got something meaningful from short summary! 
                        // Override the summary with the more condensed short summary
                        summary = shortSummary;   
                    }
                }

            }
            catch (Exception)
            {
                // TODO: Log meaningful error
            }
            return new Incident(shortSummary, summary);
        }
    }
}