namespace MCSC.Scrape
{
    public class AbRcmpScraper : IScraper
    {
        public Incident Scrape(string body)
        {
            // We don't want to set the short summary because then luis will parse it!
            return new Incident(null, body);
        }
    }
}