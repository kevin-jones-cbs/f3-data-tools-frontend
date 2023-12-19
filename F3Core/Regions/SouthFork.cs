namespace F3Core.Regions
{
    public class SouthFork : Region
    {
        public override string QueryStringValue => "southfork";
        public override string DisplayName => "South Fork";

        public override List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "The Ditch", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Stargate", City = "Placerville", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "The Rack", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Greyhound", City = "Folsom", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Linkz", City = "Cameron Park", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "tRuck Stop", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Parliament", City = "Placerville", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Denali", City = "Rescue", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Everest", City = "Folsom", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Apex", City = "Placerville", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Cougar's Den", City = "Placerville", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Cut", City = "Placerville", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The S.A.K.", City = "Folsom", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Pit", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Public", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Butcher's Block", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Claim", City = "Placerville", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Powerhouse", City = "Folsom", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "The Grid", City = "Shingle Springs", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "The Way", City = "Rotating", DayOfWeek = DayOfWeek.Sunday }
        };

        public override string TestingSpreadsheetId => "1wlc9khMeTuTxd9KS5WP-AGUVWUE8PIkOsvpXfiC7h34";

        public override string RealSpreadsheetId => "1Wc58fAiGF7HxcM5Od0g7h-g3-no6XvzTNld68j-kfWw";

        public override int MasterDataSheetId => 729344821;

        public override int RosterSheetId => 437240319;

        public override string RangeForGettingRowCount => "B11000:K";
    }
}