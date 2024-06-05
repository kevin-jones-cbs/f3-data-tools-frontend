namespace F3Core.Regions
{
    public class CapValleyIpssa : Region
    {
        public override string QueryStringValue => "capvalleyipssa";

        public override string DisplayName => "Cap Valley Ipssa";

        public List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "Monthly Meeting", City = "Fair Oaks", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Board Meeting", City = "Elk Grove", DayOfWeek = DayOfWeek.Wednesday },
        };

        

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


        public override string AosSheetName => throw new NotImplementedException();

        public override AoColumnIndicies AoColumnIndicies => throw new NotImplementedException();
        public override string AosRetiredIndicator => throw new NotImplementedException();
    }
}