namespace F3Core.Regions
{
    public class Rubicon : Region
    {
        public override string QueryStringValue => "rubicon";
        public override string DisplayName => "Rubicon";

        public List<Ao> AoList => new List<Ao>
        {
            new Ao { Name = "Redwall", City = "Auburn", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Descent", City = "Auburn", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "The Office", City = "Grass Valley", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "Agoge", City = "Auburn", DayOfWeek = DayOfWeek.Monday },
            new Ao { Name = "The Clink", City = "Auburn", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Eagle's Nest", City = "Loomis", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "The Centurion", City = "Grass Valley", DayOfWeek = DayOfWeek.Tuesday },
            new Ao { Name = "Rally Point", City = "Auburn", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "Impact", City = "Grass Valley", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Courthouse", City = "Auburn", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Basin", City = "Loomis", DayOfWeek = DayOfWeek.Wednesday },
            new Ao { Name = "The Rock", City = "Auburn", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "Gold Mine", City = "Loomis", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "Flight Deck", City = "Grass Valley", DayOfWeek = DayOfWeek.Thursday },
            new Ao { Name = "The Crown", City = "Newcastle", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Damascus", City = "Auburn", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Honor Guard", City = "Grass Valley", DayOfWeek = DayOfWeek.Friday },
            new Ao { Name = "Mother Lode", City = "Auburn", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Cardiac", City = "Auburn", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Foundry", City = "Grass Valley", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "CSAUP/Other", City = "Auburn", DayOfWeek = DayOfWeek.Saturday },
            new Ao { Name = "Canyonz", City = "Auburn", DayOfWeek = DayOfWeek.Saturday },
        };

        

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


        public override string AosSheetName => throw new NotImplementedException();

        public override AoColumnIndicies AoColumnIndicies => throw new NotImplementedException();

        public override string AosRetiredIndicator => throw new NotImplementedException();
    }
}