using F3Core;
using Xunit;

namespace F3Wasm.Tests;

public class RecentActivityTests
{
    [Fact]
    public void ApplyRecentActivity_UsesRollingWindowAndRegionalRanking()
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
        Assert.Equal(4, rows[0].HeatLevel);
        Assert.Equal(1, rows[0].HeatRank);

        Assert.Equal(1, rows[1].RecentPostCount);
        Assert.Equal(4, rows[1].HeatLevel);
        Assert.Equal(2, rows[1].HeatRank);

        Assert.Equal(0, rows[2].RecentPostCount);
        Assert.Equal(0, rows[2].HeatLevel);
        Assert.Null(rows[2].HeatRank);
    }

    [Fact]
    public void ApplyRecentActivity_AssignsPercentageTiersAfterSeparatingLeader()
    {
        var asOfDate = new DateTime(2026, 8, 23);
        var rows = Enumerable.Range(1, 11)
            .Select(index => new DisplayRow { PaxName = $"Pax {index}" })
            .ToList();
        var posts = rows
            .SelectMany((row, index) => Enumerable.Range(0, 11 - index)
                .Select(_ => new Post { Pax = row.PaxName, Date = asOfDate }))
            .ToList();

        DataHelper.ApplyRecentActivity(rows, posts, asOfDate);

        Assert.Equal(new[] { 4, 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 }, rows.Select(row => row.HeatLevel));
    }

    [Fact]
    public void ApplyRecentActivity_PromotesTiesAcrossTierBoundary()
    {
        var asOfDate = new DateTime(2026, 8, 23);
        var postCounts = new[] { 12, 10, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        var rows = postCounts
            .Select((_, index) => new DisplayRow { PaxName = $"Pax {index + 1}" })
            .ToList();
        var posts = rows
            .SelectMany((row, index) => Enumerable.Range(0, postCounts[index])
                .Select(_ => new Post { Pax = row.PaxName, Date = asOfDate }))
            .ToList();

        DataHelper.ApplyRecentActivity(rows, posts, asOfDate);

        Assert.Equal(4, rows[1].HeatLevel);
        Assert.Equal(4, rows[2].HeatLevel);
        Assert.Equal(rows[1].HeatRank, rows[2].HeatRank);
    }
}
