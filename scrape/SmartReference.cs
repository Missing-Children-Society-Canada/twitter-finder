using System;
using Microsoft.Extensions.Logging;

namespace MCSC.Parsing
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

        public bool Load(out string shortSummary, out string summary)
        {
            try
            {
                var web = new AutoDecompressWebClient();
                var data = web.DownloadString(_uri);

                data = StringSanitizer.SimplifyHtmlEncoded(data);

                var sr = new SmartReader.Reader(_uri, data);
                var article = sr.GetArticle();
                if (!string.IsNullOrEmpty(article.TextContent))
                {
                    shortSummary =
                        StringSanitizer.RemoveDoublespaces(
                                StringSanitizer.RemoveSpecialCharacters(
                                    StringSanitizer.RemoveFillerWords(article.TextContent)))
                                    .Trim();

                    summary =
                        StringSanitizer.RemoveDoublespaces(
                            StringSanitizer.RemoveHashtags(article.TextContent))
                            .Trim();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception loading article");
            }

            summary = null;
            shortSummary = null;
            return false;
        }
    }
}