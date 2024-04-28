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
            new Ao { Name = "The Oak", City = "Sacramento", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Stage", City = "Elk Grove", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The River", City = "Sacramento", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "Camp West", City = "Sacramento", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "McKinley", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Shamrock", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Refuge", City = "Elk Grove", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The District", City = "Elk Grove", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Dockyards", City = "Elk Grove", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Kings Landing", City = "Sacramento", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "EGP", City = "Elk Grove", DayOfWeek = DayOfWeek.Saturday }
        };

        public override string TestingSpreadsheetId => "1lyFtAFDJExSb9pIqh-hJ4AsOHxwR1i84UrjHVNf4bgc";

        public override string RealSpreadsheetId => "1hKQvESiXvfIHmisEY_PZn2RJ-aW_yCGsO7GSULAbZ-Y";

        public override int MasterDataSheetId => 729344821;
        public override string MasterDataSheetName => "Master Data";
        public override int MissingDataRowOffset => 2400;

        public override MasterDataColumnIndicies MasterDataColumnIndicies => new MasterDataColumnIndicies
        {
            Date = 1,
            Location = 10,
            PaxName = 11,
            Fng = 12,
            Post = 13,
            Q = 14
        };

        public override int RosterSheetId => 437240319;
        public override string RosterSheetName => "Roster";
        public override string RosterNameColumn => "B";
        public override List<RosterSheetColumn> RosterSheetColumns => new List<RosterSheetColumn>
        {
            RosterSheetColumn.Formula, RosterSheetColumn.PaxName, RosterSheetColumn.Empty, RosterSheetColumn.JoinDate, RosterSheetColumn.Formula, RosterSheetColumn.NamingRegionYN
        };
    }
}