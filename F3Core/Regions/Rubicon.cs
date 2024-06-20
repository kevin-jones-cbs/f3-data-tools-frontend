namespace F3Core.Regions
{
    public class Rubicon : Region
    {
        public override string QueryStringValue => "rubicon";
        public override string DisplayName => "Rubicon";

        public override string SpreadsheetId => "13iHs_ljdejKaV4UTmCO4v2yAuFxMFvytIvdvXUs7R2c";

        public override int MasterDataSheetId => 729344821;
        public override string MasterDataSheetName => "Master Data";
        public override int MissingDataRowOffset => 14000;

        public override MasterDataColumnIndicies MasterDataColumnIndicies => new MasterDataColumnIndicies
        {
            Date = 5,
            Location = 1,
            PaxName = 0,
            Fng = 2,
            Post = 3,
            Q = 4
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
            Retired = 11
        };

        public override string AosRetiredIndicator => string.Empty;
    }
}