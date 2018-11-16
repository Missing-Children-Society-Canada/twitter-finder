using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class UnknownSiteParser : IBodyParse
    {
        // Example: *
        public string Uri
        {
            get
            {
                return ".";
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