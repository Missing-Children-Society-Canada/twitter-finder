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
                var client = new HttpClient(handler);

				// setting the default user agent
				if (client.DefaultRequestHeaders.UserAgent.Count == 0)  {                     
					client.DefaultRequestHeaders.UserAgent.ParseAdd("Azure Function");
				}

                var httpResponseMessage = await client.GetAsync(_uri);
                httpResponseMessage.EnsureSuccessStatusCode();

                // There is a bug in the .net framework that causes ReadAsStringAsync() to fail if the server reports the content encoding as "utf-8" rather than utf-8 https://github.com/dotnet/corefx/issues/5014
                if (httpResponseMessage.Content.Headers.ContentType?.CharSet == @"""utf-8""")
                {
                    httpResponseMessage.Content.Headers.ContentType.CharSet = "UTF-8";
                }

                var data  = await httpResponseMessage.Content.ReadAsStringAsync();

                data = StringSanitizer.SimplifyHtmlEncoded(data);

                var sr = new SmartReader.Reader(_uri, data);
                sr.AddCustomOperationStart(SpaceElements);
                var article = sr.GetArticle();
				var content = !string.IsNullOrEmpty(article.TextContent) ? article.TextContent : article.Excerpt;

                if (!string.IsNullOrEmpty(content))
                {
                    var shortSummary =
                        StringSanitizer.RemoveDoublespaces(
                        StringSanitizer.RemoveUrls(
                        StringSanitizer.RemoveHashtags(content)))
                        .Trim();

                    var summary =
                        StringSanitizer.RemoveDoublespaces(content)
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