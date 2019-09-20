using System.Text.RegularExpressions;

namespace MCSC
{
    internal static class StringSanitizer
    {
        public static string RemoveHashtags(string input)
        {
            return Regex.Replace(input, @"[#]+|\\s+|\t|\n|\r", " ");
        }

        public static string RemoveDoublespaces(string input)
        {
            Regex regex = new Regex("[ |\t|\n|\r]{2,}", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            return regex.Replace(input, " ");
        }

        public static string RemoveUrls(string input)
        {
            return Regex.Replace(input, @"(?:https?):\/\/[\S]+", "");
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
            // replace common html entities with basic text 
            return input
                .Replace("&nbsp;", " ") // non breaking space
                .Replace("&quot;", "\"") // double quote
                .Replace("&middot;", ".").Replace("&#183;", ".")
                .Replace("&ndash;", "-").Replace("&#8211;", "-") // en dash
                .Replace("&mdash;", "--").Replace("&#8212;", "--") // em dash
                .Replace("&lsquo;", "'").Replace("&#8216;", "'") // left single quotation mark
                .Replace("&rsquo;", "'").Replace("&#8217;", "'") // right single quotation mark
                .Replace("&sbquo;", ",").Replace("&#8218;", ",") // single low-9 quotation mark
                .Replace("&#8219;", "'") // single high-9 reversed quotation mark
                .Replace("&ldquo;", "\"").Replace("&#8220;", "\"") // left double quotation mark
                .Replace("&rdquo;", "\"").Replace("&#8221;", "\"") // right double quotation mark
                .Replace("&bdquo;", ",,").Replace("&#8222;", ",,") // double low-9 quotation mark
                .Replace("&dagger;", "*").Replace("&#8224;", "*")
                .Replace("&bull;", "*").Replace("&#8226;", "*")
                .Replace("&hellip;", "...").Replace("&#8230;", "...") // horizontal ellipsis
                .Replace("&prime;", "'").Replace("&#8242;", "'") // prime
                .Replace("&Prime;", "\"").Replace("&#8243;", "\""); // double prime
        }

        public static string SimplifyPunctuation(string input)
        {
            return input
                .Replace('\u2013', '-') // en dash
                .Replace('\u2014', '-') // em dash
                .Replace('\u2015', '-') // horizontal bar
                .Replace('\u2017', '_') // double low line
                .Replace('\u2018', '\'') // left single quotation mark
                .Replace('\u2019', '\'') // right single quotation mark
                .Replace('\u201a', ',') // single low-9 quotation mark
                .Replace('\u201b', '\'') // single high-reversed-9 quotation mark
                .Replace('\u201c', '\"') // left double quotation mark
                .Replace('\u201d', '\"') // right double quotation mark
                .Replace('\u201e', '\"') // double low-9 quotation mark
                .Replace("\u2026", "...") // horizontal ellipsis
                .Replace('\u2032', '\'') // prime
                .Replace('\u2033', '\"'); // double prime
        }
    }
}
