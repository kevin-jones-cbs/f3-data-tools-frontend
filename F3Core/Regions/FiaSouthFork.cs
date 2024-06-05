namespace F3Core.Regions
{
    public class FiaSouthFork : Region
    {
        public override string QueryStringValue => "fiasouthfork";
        public override string DisplayName => "FiA South Fork";

        public List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "The Climb", City = "Cameron Park", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "The Bank", City = "Placerville", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Powerhouse", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Jungle", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Lab", City = "Placerville", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Powerhouse - Th", City = "El Dorado Hills", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Den", City = "Shingle Springs", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "The Grid", City = "Shingle Springs", DayOfWeek = DayOfWeek.Saturday }
        };

        

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


        public override string AosSheetName => throw new NotImplementedException();

        public override AoColumnIndicies AoColumnIndicies => throw new NotImplementedException();
        public override string AosRetiredIndicator => throw new NotImplementedException();
    }
}