namespace F3Core.Regions
{
    public enum RosterSheetColumn
    {
        PaxName, JoinDate, Empty, Formula, NamingRegionName, NamingRegionYN
    }

    public abstract class Region
    {
        public abstract string QueryStringValue { get; }
        public abstract string DisplayName { get; }
        
        public abstract List<Ao> AoList { get; }

        // Sheets
        public abstract string TestingSpreadsheetId { get; }
        public abstract string RealSpreadsheetId { get; }

        public abstract int MasterDataSheetId { get; }
        public abstract string MasterDataSheetName { get; }

        public abstract int RosterSheetId { get; }
        public abstract string RosterSheetName { get; }
        public abstract string RosterNameColumn { get; }
        public abstract List<RosterSheetColumn> RosterSheetColumns { get; }

        public abstract string RangeForGettingRowCount { get; }

        // Function to return real or testing spreadsheet id depending on isTesting bool
        public string GetSpreadsheetId(bool isTesting)
        {
            return isTesting ? TestingSpreadsheetId : RealSpreadsheetId;
        }

        // Function for getting CacheKey, accepting an isTesting bool
        public string GetCacheKey(bool isTesting)
        {
            return $"{DisplayName}-{isTesting}";
        }
    }
}