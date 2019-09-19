using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MCSC
{
    public static class IDictionaryExtensions
    {
        public static string FirstOrDefaultElement(this IDictionary<string, object> dict)
        {
            if (dict == null)
                return null;
            var first = dict.Values.First();
            JArray jArray = first as JArray;
            return jArray?.First?.Value<string>();
        }
    }
}