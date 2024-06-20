namespace F3Core.Regions
{
    public class FiaSouthFork : Region
    {
        public override string QueryStringValue => "fiasouthfork";
        public override string DisplayName => "FiA South Fork";

        public override string SpreadsheetId => "1GwwtHY1AW-h5c-PSQ1uX-YIvU1HFSZ-ksS7TstQR-yc";

        public override int MasterDataSheetId => 511807761;
        public override string MasterDataSheetName => "Master Data";
        public override int MissingDataRowOffset => 4500;

        public override MasterDataColumnIndicies MasterDataColumnIndicies => new MasterDataColumnIndicies
        {
            Date = 1,
            Location = 10,
            PaxName = 11,
            Fng = 12,
            Post = 13,
            Q = 14
        };

        public override int RosterSheetId => 92100026;
        public override string RosterSheetName => "Roster";
        public override string RosterNameColumn => "B";
        public override List<RosterSheetColumn> RosterSheetColumns => new List<RosterSheetColumn>
        {
            RosterSheetColumn.Formula, RosterSheetColumn.PaxName, RosterSheetColumn.JoinDate, RosterSheetColumn.Empty, RosterSheetColumn.Empty
        };


       public override string AosSheetName => "Sites";

        public override AoColumnIndicies AoColumnIndicies => new AoColumnIndicies
        {
            Name = 0,
            City = 2,
            DayOfWeek = 1,
            Retired = 11
        };
        public override string AosRetiredIndicator => string.Empty;
    }
}