namespace F3Core.Regions
{
    public class CapValleyIpssa : Region
    {
        public override string QueryStringValue => "capvalleyipssa";

        public override string DisplayName => "Cap Valley Ipssa";

        public override List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "Monthly Meeting", City = "Fair Oaks", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Board Meeting", City = "Elk Grove", DayOfWeek = DayOfWeek.Wednesday },
        };

        public override string TestingSpreadsheetId => "1lyFtAFDJExSb9pIqh-hJ4AsOHxwR1i84UrjHVNf4bgc";

        public override string RealSpreadsheetId => "1tHgi1AZQt5OnIoWVMOFqxyC5BqEYtSkACvmCP2fNNJU";

        public override int MasterDataSheetId => 729344821;

        public override int RosterSheetId => 437240319;

        public override string RangeForGettingRowCount => "B1:K";

        public override string MasterDataSheetName => "Master Data";

        public override string RosterSheetName => "Roster";

        public override string RosterNameColumn => "B";

        public override List<RosterSheetColumn> RosterSheetColumns => new List<RosterSheetColumn>
        {
            RosterSheetColumn.Formula, RosterSheetColumn.PaxName, RosterSheetColumn.JoinDate, RosterSheetColumn.Empty, RosterSheetColumn.NamingRegionName
        };
    }
}