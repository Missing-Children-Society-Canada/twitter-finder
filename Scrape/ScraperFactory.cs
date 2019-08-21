using System;
using System.Collections.Generic;

namespace MCSC.Scrape
{
    public sealed class ScraperFactory
    {
        private static ScraperFactory _instance;
        private static readonly object padlock = new object();

        public static ScraperFactory Instance
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new ScraperFactory());
                }
            }
        }

        private readonly IDictionary<string, Type> _parsers;

        // List of available parsers
        // If the link doesn't match any the UnknownSiteParser will be used
        private ScraperFactory()
        {
            _parsers = new Dictionary<string, Type>
            {
                {"themissing.ca", typeof(TheMissingScraper)},
                {"services.rcmp-grc", typeof(RcmpScraper)},
                {"rcmp-grc.gc.ca/ab", typeof(AbRcmpScraper)},
                {"bc.rcmp-grc.gc.ca", typeof(BcRcmpScraper)},
                {"police.brandon.ca", typeof(BrandonPoliceScraper)},
                {"peelpolice.ca", typeof(PeelPoliceScraper)},
                {"reginapolice.ca", typeof(ReginaPoliceScraper)},
                {"saskatoonpolice.ca", typeof(SaskatoonPoliceScraper)},
                {"niagarapolice.ca", typeof(NiagaraPoliceScraper)},
                {"missingpeople.ca", typeof(MissingPeopleScraper)},
                {"kingstonpolice.ca", typeof(KingstonPoliceScraper)}
            };
        }

        public IScraper BuildScraper(string url)
        {
            foreach (var keyValuePair in _parsers)
            {
                if (url.Contains(keyValuePair.Key))
                {
                    Type t = keyValuePair.Value;
                    return ((IScraper)Activator.CreateInstance(t));
                }
            }
            return null;
        }
    }
}
