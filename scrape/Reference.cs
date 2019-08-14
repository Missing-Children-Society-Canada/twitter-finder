using System;
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
                var web = new MyWebClient();
                var data = web.DownloadString(this.uri);
                var doc = new HtmlDocument();
                doc.LoadHtml(data);

                // The body is where the majority of the content is so use that
                return doc.DocumentNode
                    .SelectNodes("//body")
                    .First()
                    .InnerHtml;
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

    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }
}