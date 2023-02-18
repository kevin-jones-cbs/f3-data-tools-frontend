namespace F3Core.Regions
{
    public class Asgard : Region
    {
        public override string QueryStringValue => "asgard";

        public override string DisplayName => "Asgard";

        public override List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "The Boneyard", City = "Sacramento", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Training Ground", City = "Antelope", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Airstrip", City = "Rancho Cordova", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Bifrost", City = "Citrus Heights", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Garden", City = "Fair Oaks", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Sandy Garden", City = "Fair Oaks", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Ring - Th", City = "Fair Oaks", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Iron Jungle - Fr", City = "Sacramento", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Break", City = "Sacramento", DayOfWeek = DayOfWeek.Saturday }
        };

        public override string TestingSpreadsheetId => "1lyFtAFDJExSb9pIqh-hJ4AsOHxwR1i84UrjHVNf4bgc";

        public override string RealSpreadsheetId => "1lMvACqxqatH-lyJg2DrAhePFAgzPX-uErNvhaG9j0d4";

        public override int MasterDataSheetId => 729344821;

        public override int RosterSheetId => 437240319;

        public override string RangeForGettingRowCount => "B2400:K";

    }
}