using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AngleSharp.Dom;

namespace MCSC
{
    public class SmartReference
    {
        private readonly string _uri;
        private readonly ILogger _logger;

        public SmartReference(string uri, ILogger logger)
        {
            this._uri = uri;
            this._logger = logger;
        }

        public async Task<Incident> LoadAsync()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };
                var web = new HttpClient(handler);
                var data = await web.GetStringAsync(_uri);

                data = StringSanitizer.SimplifyHtmlEncoded(data);

                var sr = new SmartReader.Reader(_uri, data);
                sr.AddCustomOperationStart(SpaceElements);
                var article = sr.GetArticle();
                if (!string.IsNullOrEmpty(article.TextContent))
                {
                    var shortSummary =
                        StringSanitizer.RemoveDoublespaces(
                        StringSanitizer.RemoveUrls(
                        StringSanitizer.RemoveHashtags(article.TextContent)))
                        .Trim();

                    var summary =
                        StringSanitizer.RemoveDoublespaces(article.TextContent)
                        .Trim();
                    return new Incident(shortSummary, summary);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception loading article");
            }

            return null;
        }
        
        // space out certain elements so that the article text receives these as sentence breaks
        // input such as <li>abc</li><li>def</li> should be 'abc\r\n def' rather than 'abcdef'
        private static void SpaceElements(IElement element)
        {
            foreach (var c in element.QuerySelectorAll("li"))
            {
                c.InnerHtml = "\r\n" + c.InnerHtml;
            }
            foreach (var c in element.QuerySelectorAll("p"))
            {
                c.InnerHtml = "\r\n" + c.InnerHtml;
            }
        }
    }
}