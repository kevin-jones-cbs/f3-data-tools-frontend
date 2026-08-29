using F3Core;
using Xunit;

namespace F3Wasm.Tests;

public class RecentActivityTests
{
    [Fact]
    public void ApplyRecentActivity_UsesRollingWindowAndRegionalNormalization()
    {
        var asOfDate = new DateTime(2026, 8, 23);
        var rows = new List<DisplayRow>
        {
            new() { PaxName = "RoBlox" },
            new() { PaxName = "Pixar" },
            new() { PaxName = "Citation" }
        };
        var posts = new List<Post>
        {
            new() { Pax = "RoBlox", Date = asOfDate },
            new() { Pax = "roblox", Date = asOfDate.AddDays(-29) },
            new() { Pax = "RoBlox", Date = asOfDate.AddDays(-30) },
            new() { Pax = "RoBlox", Date = asOfDate, Site = "UPDATE" },
            new() { Pax = "Pixar", Date = asOfDate.AddDays(-10) }
        };

        DataHelper.ApplyRecentActivity(rows, posts, asOfDate);

        Assert.Equal(2, rows[0].RecentPostCount);
        Assert.Equal(100, rows[0].HeatScore);
        Assert.Equal(1, rows[0].HeatRank);

        Assert.Equal(1, rows[1].RecentPostCount);
        Assert.Equal(50, rows[1].HeatScore);
        Assert.Equal(2, rows[1].HeatRank);

        Assert.Equal(0, rows[2].RecentPostCount);
        Assert.Equal(0, rows[2].HeatScore);
        Assert.Null(rows[2].HeatRank);
    }
}
