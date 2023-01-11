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

        public static List<Pax> GetPaxFromComment(string comment, List<string> allPax)
        {
            // Remove all spaces, commas and new lines.
            comment = comment.StringEscaped();

            MatchCollection matches = Regex.Matches(comment, @"@\w+");

            var pax = new List<Pax>();

            foreach (var match in matches)
            {
                var paxName = match.ToString().Replace("@", string.Empty).ToLower();

                // check if main name matches or if 2.0 matches
                var matchingPax = allPax.FirstOrDefault(x => new List<string> { paxName.ToLower(), $"{paxName.ToLower()}(20)" }.Contains(
                    x.StringEscaped()));
                if (matchingPax != null)
                {
                    pax.Add(new Pax { Name = matchingPax, UnknownName = "", IsOfficial = true });
                }
                else
                {
                    pax.Add(new Pax { UnknownName = paxName, Name = "", IsOfficial = false });

                }
            }

            // Use regex to find the text following Q:
            var qMatch = Regex.Match(comment, @"q:?@?\w+");

            // If we found a Q, then set the isQ for that pax
            if (qMatch.Success)
            {
                var qName = qMatch.ToString()
                    .StringEscaped()
                    .Replace("q:", string.Empty)
                    .Replace("@", string.Empty);
                var matchingPax = pax.FirstOrDefault(x => x.Name != null && x.Name
                    .StringEscaped().Contains(qName));
                if (matchingPax != null)
                {
                    matchingPax.IsQ = true;
                }
            }

            return pax;
        }
    }

    // Extension class for a string

    public static class StringExtensions
    {
        public static string StringEscaped(this string input)
        {
            return input.Replace(" ", string.Empty)
                        .Replace(".", string.Empty)
                        .Replace("’", string.Empty)
                        .Replace("'", string.Empty)
                        .Replace("\r\n", string.Empty)
                        .ToLower();
        }
    }

}
