using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;

using MCSC.Classes;
namespace MCSC.Parsing
{
    public class BCRCMPParser : IBodyParse
    {
        // Example: http://bc.rcmp-grc.gc.ca/ViewPage.action?siteNodeId=2087&languageId=1&contentId=57000
        public string Uri
        {
            get
            {
                
                // TODO: Test and fix
                // This website was doing a redirect that was messing up the parsing
                return "bc.rcmp-grc.gc.ca";
            }
        }

        public Incident Parse(string body)
        {
            return new Incident()
            {
                // We don't want to set the short summary because then luis will parse it!
                // ShortSummary = body,
                Summary = body
            };
        }
    }
}