using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class NiagaraPoliceParser : IBodyParse
    {
        // Example: https://www.niagarapolice.ca/en/news/index.aspx?newsId=9af514bb-6058-419a-bae2-13e2841aa269
        public string Uri
        {
            get
            {
                return "niagarapolice.ca";
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
                var node = document.GetElementbyId("news_content");
                if (node != null)
                {
                    shortSummary = node.InnerHtml;
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