namespace F3Core.Regions
{
    public class Asgard : Region
    {
        public override string QueryStringValue => "asgard";

        public override string DisplayName => "Asgard";

        public override string SpreadsheetId => "1lMvACqxqatH-lyJg2DrAhePFAgzPX-uErNvhaG9j0d4";

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


        public override string AosSheetName => "Sites";

        public override AoColumnIndicies AoColumnIndicies => new AoColumnIndicies
        {
            Name = 1,
            City = 3,
            DayOfWeek = 2,
            Retired = 12
        };
        public override string AosRetiredIndicator => string.Empty;
    }
}