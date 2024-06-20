namespace F3Core.Regions
{
    public class Delta : Region
    {
        public override string QueryStringValue => "delta";

        public override string DisplayName => "Delta";        

        public override string SpreadsheetId => "1hKQvESiXvfIHmisEY_PZn2RJ-aW_yCGsO7GSULAbZ-Y";

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


        public override string AosSheetName => "Sites";

        public override AoColumnIndicies AoColumnIndicies => new AoColumnIndicies
        {
            Name = 1,
            City = 3,
            DayOfWeek = 2,
            Retired = 7
        };
        public override string AosRetiredIndicator => string.Empty;
    }
}