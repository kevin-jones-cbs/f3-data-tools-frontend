namespace F3Core.Regions
{
    public abstract class Region
    {
        public abstract string QueryStringValue { get; }
        public abstract string DisplayName { get; }
        
        public abstract List<Ao> AoList { get; }

        // Sheets
        public abstract string TestingSpreadsheetId { get; }
        public abstract string RealSpreadsheetId { get; }
        public abstract int MasterDataSheetId { get; }
        public abstract int RosterSheetId { get; }
        public abstract string RangeForGettingRowCount { get; }

        // Function for getting CacheKey, accepting an isTesting bool
        public string GetCacheKey(bool isTesting)
        {
            return $"{DisplayName}-{isTesting}";
        }
    }
}