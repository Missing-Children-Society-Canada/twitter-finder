using MCSC.Classes;
namespace MCSC.Parsing
{
    public interface IBodyParse
    {
        string Uri
        {
            get;
        }
        Incident Parse(string body);
    }
}