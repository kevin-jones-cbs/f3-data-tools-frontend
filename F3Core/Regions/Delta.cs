namespace F3Core.Regions
{
    public class Delta : Region
    {
        public override string QueryStringValue => "delta";

        public override string DisplayName => "Delta";

        public override List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "Shasta", City = "Elk Grove", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Statehouse", City = "Sacramento", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "The Rock", City = "Sacramento", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Creek", City = "Elk Grove", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Oasis", City = "Elk Grove", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Mainline", City = "Elk Grove", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The American", City = "Sacramento", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Stage", City = "Elk Grove", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The River", City = "Sacramento", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "Camp West", City = "Sacramento", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "McKinley", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Shamrock", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The District", City = "Elk Grove", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Dockyards", City = "Elk Grove", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Kings Landing", City = "Sacramento", DayOfWeek = DayOfWeek.Saturday }
        };

        public override string TestingSpreadsheetId => "1lyFtAFDJExSb9pIqh-hJ4AsOHxwR1i84UrjHVNf4bgc";

        public override string RealSpreadsheetId => "178rhTFHiYSOxs5xyM7G5moZOPFDVbZ3H35FORj45XNc";

        public override int MasterDataSheetId => 0;

        public override int RosterSheetId => 124316301;

        public override string RangeForGettingRowCount => "B2400:K";

    }
}