namespace F3Core.Regions
{
    public class Asgard : Region
    {
        public override string QueryStringValue => "asgard";

        public override string DisplayName => "Asgard";

        public override List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "The Boneyard", City = "Sacramento", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Pow Wow - M", City = "Orangevale", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Airstrip", City = "Rancho Cordova", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Training Ground - T", City = "Antelope", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Mammoth", City = "Sacramento", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Pound - W", City = "Citrus Heights", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Ring - Th", City = "Fair Oaks", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "Sandy Garden - Th", City = "Fair Oaks", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Iron Jungle - Fr", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Beavertown", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Break", City = "Sacramento", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "The Yard", City = "Folsom", DayOfWeek = DayOfWeek.Saturday }
        };

        public override string TestingSpreadsheetId => "1lyFtAFDJExSb9pIqh-hJ4AsOHxwR1i84UrjHVNf4bgc";

        public override string RealSpreadsheetId => "1lMvACqxqatH-lyJg2DrAhePFAgzPX-uErNvhaG9j0d4";

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
            RosterSheetColumn.Formula, RosterSheetColumn.PaxName, RosterSheetColumn.JoinDate, RosterSheetColumn.Empty, RosterSheetColumn.NamingRegionName
        };
    }
}