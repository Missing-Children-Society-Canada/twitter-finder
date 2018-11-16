using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Linq;
using System.Net;
using MCSC.Classes;

namespace MCSC.Parsing
{
    public class Reference
    {
        private readonly string uri;
        private readonly Parser parser;

        public Reference(string uri)
        {
            this.uri = uri.ToLower();
            this.parser = new Parser(uri);
        }

        public string Body()
        {
            try
            {
                var web = new HtmlWeb()
                {
                    UseCookies = true,
                    CaptureRedirect = true
                };
                
                var doc = web.Load(this.uri);
                if ((web.StatusCode != HttpStatusCode.OK))
                {
                    // TO DO: Error Logging something went wrong
                    // BC RCMP Pages have a redirect that blocks us from parsing so just return an empty page
                    return string.Empty;
                }
                else 
                {
                    // The body is where the majority of the content is so use that
                    return doc.DocumentNode
                    .SelectNodes("//body")
                    .First()
                    .InnerHtml;
                }
            }
            catch (Exception e)
            {
                // TO DO: Error Logging something went wrong
                return string.Empty;
            }
        }

        public Incident Load()
        {
            var body = this.Body();
            if (String.IsNullOrEmpty(body))
            {
                // TO DO: Error Logging something went wrong
                return new Incident();
            }
            return this.parser.Parse(body);
        }
    }
}