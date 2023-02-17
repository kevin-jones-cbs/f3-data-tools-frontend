using F3Wasm.Models;
using System.Text.RegularExpressions;

namespace F3Wasm.Data
{
    public static class PaxHelper
    {
        // Create a mapping of names
        private static Dictionary<string, string> NameMapping = new Dictionary<string, string>
        {
            { "Manny Pedi", "Mani Pedi" },
            { "Roxbury", "ROXBURY mike c" },
            { "SweatShop", "SweatShop - Hernan C" },
            { "Mr. Meanor", "Mr. Meaner" },
            { "Linguini", "Linguine" },
            { "Hillbilly", "Hill Billy" },
            { "HeatCheck", "Heat Check" },
        };  

        public static List<Pax> GetPaxFromComment(string comment, List<string> allPax)
        {
            comment = comment.NewStringEscaped();
            allPax = allPax.Distinct().ToList();

            // Pax dictionary where value is .StringEscaped
            var paxDictionary = allPax.ToDictionary(x => x, x => x.NewStringEscaped().Replace(" ", @"\s?"));
            
            // Combine with the Name Mapping, replace key if it exists
            foreach (var nameMapping in NameMapping)
            {
                if (paxDictionary.ContainsKey(nameMapping.Key))
                {
                    paxDictionary[nameMapping.Key] = nameMapping.Value.NewStringEscaped().Replace(" ", @"\s?");
                }
                else
                {
                    paxDictionary.Add(nameMapping.Key, nameMapping.Value.NewStringEscaped().Replace(" ", @"\s?"));
                }
            }            

            var allPaxRegex = new Regex(@$"\b({string.Join("|", paxDictionary.Values)})\b", RegexOptions.IgnoreCase);

            // Get matches from comment
            var matches = allPaxRegex.Matches(comment);

            var pax = new List<Pax>();
            
            foreach (Match match in matches)
            {
                // Check the dictionary for a match to the value
                var paxName = paxDictionary
                    .FirstOrDefault(x => match.Value.ToLower() == x.Value.ToLower() || 
                                         match.Value.ToLower() == x.Value.ToLower().Replace(@"\s?", "") || 
                                         match.Value.ToLower() == x.Value.ToLower().Replace(@"\s?", " ")).Key;

                // If we found a match, then add it to the list
                if (paxName != null)
                {
                    pax.Add(new Pax { Name = paxName, UnknownName = match.Value, IsOfficial = true });
                }
                else
                {
                    pax.Add(new Pax { UnknownName = match.Value, IsOfficial = false });
                }
            }

            // Remove anything that was found from comment
            foreach (var p in pax)
            {
                comment = comment.Replace(p.UnknownName, string.Empty);
                p.UnknownName = string.Empty;
            }

            var regexHotwordPattern = @"\b(VQ|Q|FNG|PAX List|PAX|The Ditch|Stargate|Greyhound|The Linkz|Denali|Everest|The Cut|The S.A.K.|The Sak|The Public|Butcher's Block|The Claim}Powerhouse|The Grid|(\d*))\b";
            // Trim the comment, remove any @'s, then string split on spaces, 
            comment = comment.Replace("@", string.Empty).Trim();
            var commentSplit = Regex.Replace(comment, regexHotwordPattern, string.Empty, RegexOptions.IgnoreCase)
                .Replace("\r\n", string.Empty)
                .Split(' ')
                .Where(x => x.Length > 1)
                .Where(x => Regex.IsMatch(x, @"[a-zA-Z]")) // Ensure each item in commentSplit has at least one letter
                .ToList();

            // consider each of these as a pax with an unknown name
            foreach (var c in commentSplit)
            {
                pax.Add(new Pax { UnknownName = c, IsOfficial = false });
            }

            return pax;
        }
    }

    public static class StringExtensions
    {
        public static string NewStringEscaped(this string input)
        {
            return input.Replace("’", "'");
        }
    }
}
