using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class Parser : IBodyParse
    {
        public string Uri
        {
            get
            {
                return null;
            }
        }

        private readonly IBodyParse parser = new UnknownSiteParser();
        private static readonly IDictionary<string, IBodyParse> parsers;
        
        // List of available parsers
        // If the link doesn't match any the UnknownSiteParser will be used
        static Parser()
        {
            parsers = new Dictionary<string, IBodyParse>(7);
            IBodyParse p = new TheMissingParser();
            parsers.Add(p.Uri, p);
            p = new RCMPParser();
            parsers.Add(p.Uri, p);
            p = new ALBRCMPParser();
            parsers.Add(p.Uri, p);
            p = new BCRCMPParser();
            parsers.Add(p.Uri, p);
            p = new BrandonPoliceServicesParser();
            parsers.Add(p.Uri, p);
            p = new PeelRegionalPoliceParser();
            parsers.Add(p.Uri, p);
            p = new ReginaPoliceParser();
            parsers.Add(p.Uri, p);
            p = new SaskatoonPoliceParser();
            parsers.Add(p.Uri, p);
            p = new NiagaraPoliceParser();
            parsers.Add(p.Uri, p);
            p = new MissingPeopleParser();
            parsers.Add(p.Uri, p);
        }

        public Parser(string url)
        {
            foreach (var uri in parsers.Keys)
            {
                if (url.Contains(uri))
                {
                    this.parser = parsers[uri];
                }
            }
        }
        // Clean up for the summary
        // The summary is the human readable content that is displayed in the ESRI portal 
        public string SummaryCleanUp(string body)
        {
            //Console.WriteLine("Inside Clean up!!! \n");
            // Remove all tags
            string result = Regex.Replace(body, "<.*?>", " ");
    
            result = result.Replace("&nbsp;", " ");
            
            result = Regex.Replace(result, @"[#]+|\\s+|\t|\n|\r", " ");
        
            // Remove all extra spaces
            var options = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant;
            Regex regex = new Regex("[ ]{2,}", options);
            result = regex.Replace(result, " ");

            result = result.Trim();
            return result;
        }

        // Clean up for the ShortSummary
        // The short summary is the shortened version of the summary that is optimized to be processed by LUIS
        public string ShortSummaryCleanUp(string body)
        {
            // Remove all tags
            string result = Regex.Replace(body, "<.*?>", " ");
            result = result.ToLower();
            result = result.Replace("&nbsp;", " ");
            result = result.Replace("&quot;", " ");
            result = result.Replace("&ndash;", " ");
            result = result.Replace("&rsquo;", " ");

            string[] FillerWords = {"a", "and", "the", "on","or", "at", "was", "with","light","contact", "nearest", "detachment",
            "failed", "investigations", "anyone", "regarding", "approximately", "dark","in", "is", "uknown", "time", "of", "any", "to", "have", "seen",
            "if", "UnknownClothing", "applicable", "UnknownFile","it" , "unknownclothing", "information","unknownfile", "police", "service", "call", "crime",
             "stoppers", "from","by", "all", "also", "that", "his", "please", "been", "this", "concern","they","are","they","as","had","wearing", "color", "colour", "eye","shirt", "pants","be", "believed",   
             "guardians", "network", "coordinated", "response", "brown", "red", "blue", "black","without","complexion", "has", "for", "well-being", "there", "included", "release", "picture", "family", "younger", "shorts", "described", "reported", "eyes", "police", "officer", 
             "public", "attention", "asked", "live", "own", "complexity", "hair", "victimize", "children", "child", "nations", "when", "person", "jeans", "shoes", "thin", "area", "road", "criminal", "investigation", "division", "concerned", "concern", "build", "assistance", 
             "seeking", "locate", "locating", "stripe", "stripes", "straight", "short", "requesting", "request", "requests", "facebook", "twitter", "avenue", "road", "street", "large", "long", "tiny", "hoodie", "leggings", "sweater", "jacket", "boots", "tennis shoes", "leather", "worried",
             "backpack", "purse", "whereabouts", "unknown", "help", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday", "block", "crossing",
             "harm", "not", "cap", "baseball", "hat", "danger", "described", "vunerable", "picture", "friend", "thinks", "things", "media", "year", "about", "providers", "cash", "unsuccessful", "attempts", 
             "accurately", "slimmer", "slightly", "however", "nevertheless", "nike", "adidas", "puma", "joggers"};


            result = Regex.Replace(result, @"\b" + string.Join("\\b|\\b", FillerWords) + "\\b", "");
            result = Regex.Replace(result, @"[&.;()@#~_]+|\\s+|\t|\n|\r", " ");
        
            // Remove all extra spaces
            var options = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant;
            Regex regex = new Regex("[ ]{2,}", options);
            result = regex.Replace(result, " ");

           return result.Trim();
        }


        public Incident Parse(string body)
        {
            string cutdownbody = "";
            try {

                // Load the document
                var document = new HtmlDocument();
                document.LoadHtml(body);

                // The script and Style tags are never useful so remove them before more specific processing
                document.DocumentNode.Descendants()
                    .Where(n => n.Name == "script" || n.Name == "style")
                    .ToList()
                    .ForEach(n => n.Remove());
                
                // Cutdown body is the body just without Style and script tags
                cutdownbody = document.DocumentNode.InnerHtml;
            }
            catch (Exception e)
            {
                // TODO: Proper Error handling!!!
                cutdownbody = body; 
            }


            var incident = this.parser.Parse(cutdownbody);

            var shortSummary = "";
            var summary = "";

            if (!String.IsNullOrEmpty(incident.ShortSummary))
            {
                // If the short summary is available then cut it down to be optimized for LUIS
                shortSummary = this.ShortSummaryCleanUp(incident.ShortSummary);
            }
            
            if (!String.IsNullOrEmpty(incident.Summary))
            {
                // If the summary is available then cut it down to make it more human readable
                summary = this.SummaryCleanUp(incident.Summary);
            }
            return new Incident()
            {
                ShortSummary = shortSummary,
                Summary = summary
            };
        }
    }
}