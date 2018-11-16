using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions; 
using HtmlAgilityPack;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class ALBRCMPParser : IBodyParse
    {
        
        public string Uri
        {
            get
            {
                // TODO: Test and fix
                // This website was doing a redirect that was messing up the parsing
                return "rcmp-grc.gc.ca/ab";
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