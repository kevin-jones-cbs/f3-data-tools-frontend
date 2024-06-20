namespace F3Core.Regions
{
    public class CapValleyIpssa : Region
    {
        public override string QueryStringValue => "capvalleyipssa";

        public override string DisplayName => "Cap Valley Ipssa";

        public override string SpreadsheetId => "1tHgi1AZQt5OnIoWVMOFqxyC5BqEYtSkACvmCP2fNNJU";

        public override int MasterDataSheetId => 729344821;
        public override int MissingDataRowOffset => 1;

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

        public override string MasterDataSheetName => "Master Data";

        public override string RosterSheetName => "Roster";

        public override string RosterNameColumn => "B";

        public override List<RosterSheetColumn> RosterSheetColumns => new List<RosterSheetColumn>
        {
            RosterSheetColumn.Formula, RosterSheetColumn.PaxName, RosterSheetColumn.JoinDate, RosterSheetColumn.Empty, RosterSheetColumn.NamingRegionName
        };


        public override string AosSheetName => "Sites";

        public override AoColumnIndicies AoColumnIndicies => new AoColumnIndicies
        {
            Name = 1,
            City = 3,
            DayOfWeek = 2,
            Retired = 13
        };

        public override string AosRetiredIndicator => string.Empty;
    }
}