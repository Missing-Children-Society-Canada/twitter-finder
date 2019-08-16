using System.Text.RegularExpressions;

namespace MCSC.Parsing
{
    internal static class StringSanitizer
    {
        public static string RemoveHashtags(string input)
        {
            return Regex.Replace(input, @"[#]+|\\s+|\t|\n|\r", " ");
        }

        public static string RemoveDoublespaces(string input)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            return regex.Replace(input, " ");
        }

        public static string RemoveFillerWords(string input)
        {
            string[] fillerWords =
            {
                "a", "and", "the", "on", "or", "at", "was", "with", "contact", "nearest", "detachment",
                "failed", "investigations", "anyone", "regarding", "approximately", "in", "is", "uknown", "time", "of",
                "any", "to", "have", "seen",
                "if", "UnknownClothing", "applicable", "UnknownFile", "it", "unknownclothing", "information",
                "unknownfile", "police", "service", "call", "crime",
                "stoppers", "from", "by", "all", "also", "that", "his", "please", "been", "this", "concern", "they",
                "are", "they", "as", "had", "wearing",
                "color", "colour", "shirt", "pants", "be", "believed", "guardians", "network", "coordinated",
                "response", "without", "complexion",
                "has", "for", "well-being", "there", "included", "release", "picture", "family", "younger", "shorts",
                "described", "reported", "police", "officer",
                "public", "attention", "asked", "live", "own", "complexity", "victimize", "children", "child",
                "nations", "when", "person", "jeans", "shoes", "thin",
                "area", "road", "criminal", "investigation", "division", "concerned", "concern", "build", "assistance",
                "seeking", "locate", "locating", "stripe",
                "stripes", "straight", "requesting", "request", "requests", "facebook", "twitter", "avenue", "road",
                "street", "large", "tiny", "hoodie",
                "leggings", "sweater", "jacket", "boots", "tennis shoes", "leather", "worried", "backpack", "purse",
                "whereabouts", "unknown", "help",
                "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday", "block", "crossing",
                "harm", "not",
                "danger", "described", "vunerable", "picture", "friend", "thinks", "things", "media", "year", "about",
                "providers", "cash", "unsuccessful", "attempts",
                "accurately", "slimmer", "slightly", "however", "nevertheless", "nike", "adidas", "puma", "joggers"
            };

            return Regex.Replace(input.ToLower(), @"\b" + string.Join("\\b|\\b", fillerWords) + "\\b", "");
        }

        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", " ");
        }

        public static string RemoveSpecialCharacters(string input)
        {
            return Regex.Replace(input, @"[&.;()@#~_]+|\\s+|\t|\n|\r", " ");
        }

        public static string SimplifyHtmlEncoded(string input)
        {
            return input.Replace("&nbsp;", " ")
                .Replace("&quot;", " ")
                .Replace("&ndash;", " ")
                .Replace("&rsquo;", " ")
                .Replace("&lsquo;", "'")
                .Replace("&#8217;", "'")
                .Replace("&#8243;", "\"");
        }
    }
}
