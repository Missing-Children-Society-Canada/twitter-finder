namespace MCSC.Scrape
{
    public interface IScraper
    {
        Incident Scrape(string body);
    }
}