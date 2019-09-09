namespace MCSC
{
    public class Incident
    {
        public Incident(string shortSummary, string summary)
        {
            ShortSummary = shortSummary;
            Summary = summary;
        }

        // The short summary is the summary condensed to be parsed by LUIS
        // If parsing failed, or there was no parser written Short Summary may be null or empty
        public string Summary { get; }
        // The summary is what was pulled from the Body of the webpage
        // May be the entire body, may be a more speciifc part of the page or may be null or empty if there was an error
        public string ShortSummary { get; }

        public override string ToString() => $"ShortSummary: {this.ShortSummary}";
    }
}