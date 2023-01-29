using F3Wasm.Models;
using System.Text.RegularExpressions;

namespace F3Wasm.Data
{
    public static class PaxHelper
    {
        public static List<Ao> AllAos = new List<Ao>
        {
            new Ao { Name = "The Ditch", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Stargate", City = "Placerville", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Greyhound", City = "Folsom", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Linkz", City = "Cameron Park", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Denali", City = "Rescue", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Everest", City = "Folsom", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Cut", City = "Placerville", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The S.A.K.", City = "Folsom", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Public", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Butcher's Block", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Claim", City = "Placerville", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Powerhouse", City = "Folsom", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "The Grid", City = "Shingle Springs", DayOfWeek = DayOfWeek.Saturday }
        };

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

            // // Remove all spaces, commas and new lines.
            // comment = comment.StringEscaped();

            // MatchCollection matches = Regex.Matches(comment, @"@\w+");

            // var pax = new List<Pax>();

            // foreach (var match in matches)
            // {
            //     var paxName = match.ToString().Replace("@", string.Empty).ToLower();

            //     // check if main name matches or if 2.0 matches
            //     var matchingPax = allPax.FirstOrDefault(x => new List<string> { paxName.ToLower(), $"{paxName.ToLower()}(20)" }.Contains(
            //         x.StringEscaped()));
            //     if (matchingPax != null)
            //     {
            //         pax.Add(new Pax { Name = matchingPax, IsOfficial = true });
            //     }
            //     else if (NameMapping.ContainsKey(paxName))
            //     {
            //         pax.Add(new Pax { Name = NameMapping[paxName], IsOfficial = true });
            //     }
            //     else
            //     {
            //         pax.Add(new Pax { UnknownName = paxName, IsOfficial = false });
            //     }
            // }

            // // Use regex to find the text following Q:
            // var qMatch = Regex.Match(comment, @"q:?@?\w+");

            // // If we found a Q, then set the isQ for that pax
            // if (qMatch.Success)
            // {
            //     var qName = qMatch.ToString()
            //         .StringEscaped()
            //         .Replace("q:", string.Empty)
            //         .Replace("@", string.Empty);
            //     var matchingPax = pax.FirstOrDefault(x => x.Name != null && x.Name
            //         .StringEscaped().Contains(qName));
            //     if (matchingPax != null)
            //     {
            //         matchingPax.IsQ = true;
            //     }
            // }

            // return pax;
        }
    }

    // Extension class for a string

    public static class StringExtensions
    {
        public static string NewStringEscaped(this string input)
        {
            return input.Replace("’", "'");
        }
    }

}
