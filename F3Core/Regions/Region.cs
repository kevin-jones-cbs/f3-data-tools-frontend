namespace F3Core.Regions
{
    public enum RosterSheetColumn
    {
        PaxName, JoinDate, Empty, Formula, NamingRegionName, NamingRegionYN
    }

    public class AoColumnIndicies
    {
        public short Name { get; set; }
        public short City { get; set; }
        public short DayOfWeek { get; set; }
        public short Retired { get; set; }
    }

    public class MasterDataColumnIndicies
    {
        public short Date { get; set; }
        public short Location { get; set; }
        public short PaxName { get; set; }
        public short Fng { get; set; }
        public short Post { get; set; }
        public short Q { get; set; }
    }

    public abstract class Region
    {
        public abstract string QueryStringValue { get; }
        public abstract string DisplayName { get; }
        
        // Sheets
        public abstract string SpreadsheetId { get; }

        public abstract int MasterDataSheetId { get; }
        public abstract string MasterDataSheetName { get; }
        public abstract int MissingDataRowOffset { get; }
        public abstract MasterDataColumnIndicies MasterDataColumnIndicies { get; }

        public abstract int RosterSheetId { get; }
        public abstract string RosterSheetName { get; }
        public abstract string RosterNameColumn { get; }
        public abstract List<RosterSheetColumn> RosterSheetColumns { get; }

        public abstract string AosSheetName { get; }
        public abstract AoColumnIndicies AoColumnIndicies { get; }
        public abstract string AosRetiredIndicator { get; }

        public virtual bool SupportsDownrange { get; } = false;
    }
}