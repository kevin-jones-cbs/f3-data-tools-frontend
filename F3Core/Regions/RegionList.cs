namespace F3Core.Regions
{
    public static class RegionList
    {
        public static List<Region> All = new List<Region>
        {
            new SouthFork(),
            new Asgard()
        };

        // Get the region by the query string value
        public static Region GetRegion(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                return null;
            }
            
            return All.FirstOrDefault(r => r.QueryStringValue == region);
        }
    }
}