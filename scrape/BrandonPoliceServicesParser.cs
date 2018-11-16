using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class BrandonPoliceServicesParser : IBodyParse
    {
        
        public string Uri
        {
            get
            {
                return "police.brandon.ca";
            }
        }

        public Incident Parse(string body)
        {
             // Load the document
            var document = new HtmlDocument();
            document.LoadHtml(body);
            var summary = "";
            var shortSum = "";
            try
            {
                var node = document.DocumentNode.SelectNodes("//article[@class='item-pagemediaReleases']/p/span/text()");
                if (node != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node2 in node)
                        shortSum = shortSum + node2.InnerHtml;
                        
                    summary = shortSum;
                }
                node = document.DocumentNode.SelectNodes("//article[@class='item-pagemediaReleases']/p/text()");
                if (node != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node2 in node)
                        shortSum = shortSum + node2.InnerHtml;

                    summary = summary + shortSum;   
                }
                
            }
            catch(Exception e){
                //TODO: log could not parse page. 
                summary = body;
            }
            
            return new Incident()
            {
                ShortSummary = shortSum,
                Summary = summary

            };
        }
    }
}