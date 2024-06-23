namespace F3Core.Regions
{
    public class SouthFork : Region
    {
        public override string QueryStringValue => "southfork";
        public override string DisplayName => "South Fork";        

        public override string SpreadsheetId => "1Wc58fAiGF7HxcM5Od0g7h-g3-no6XvzTNld68j-kfWw";

        public override int MasterDataSheetId => 729344821;
        public override string MasterDataSheetName => "Master Data";
        public override int MissingDataRowOffset => 24000;

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


        public override string AosSheetName => "Sites";

        public override AoColumnIndicies AoColumnIndicies => new AoColumnIndicies
        {
            Name = 1,
            City = 3,
            DayOfWeek = 2,
            Retired = 14
        };

        public override string AosRetiredIndicator => string.Empty;

        public override bool SupportsDownrange => true;
    }
}