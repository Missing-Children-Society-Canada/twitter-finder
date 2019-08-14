using System.Collections.Generic;

namespace MCSC.Parsing
{
    public sealed class ParserFactory
    {
        private static ParserFactory _instance;
        private static readonly object padlock = new object();

        public static ParserFactory Instance
        {
            get
            {
                lock (padlock)
                {
                    return _instance ?? (_instance = new ParserFactory());
                }
            }
        }

        private readonly IDictionary<string, IBodyParse> _parsers;

        // List of available parsers
        // If the link doesn't match any the UnknownSiteParser will be used
        private ParserFactory()
        {
            _parsers = new Dictionary<string, IBodyParse>(7);
            IBodyParse p = new TheMissingParser();
            _parsers.Add(p.Uri, p);
            p = new RCMPParser();
            _parsers.Add(p.Uri, p);
            p = new ALBRCMPParser();
            _parsers.Add(p.Uri, p);
            p = new BCRCMPParser();
            _parsers.Add(p.Uri, p);
            p = new BrandonPoliceServicesParser();
            _parsers.Add(p.Uri, p);
            p = new PeelRegionalPoliceParser();
            _parsers.Add(p.Uri, p);
            p = new ReginaPoliceParser();
            _parsers.Add(p.Uri, p);
            p = new SaskatoonPoliceParser();
            _parsers.Add(p.Uri, p);
            p = new NiagaraPoliceParser();
            _parsers.Add(p.Uri, p);
            p = new MissingPeopleParser();
            _parsers.Add(p.Uri, p);
        }

        public IBodyParse BuildParser(string url)
        {
            foreach (var uri in _parsers.Keys)
            {
                if (url.Contains(uri))
                {
                    return _parsers[uri];
                }
            }
            return new UnknownSiteParser();
        }
    }
}
