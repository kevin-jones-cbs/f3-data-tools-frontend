using System.Text.Json;

namespace F3Wasm.Models;

public record AnalyticsChatMessage(string Role, string Content);
public sealed class AnalyticsChatReply
{
    public string Answer { get; set; } = "";
    public List<AnalyticsVisualization> Visualizations { get; set; } = [];
    public AnalyticsSnapshot? Snapshot { get; set; }
}
public sealed class AnalyticsVisualization
{
    // Table is the first renderer. Future pie/bar/line specs can share this
    // structured data and add series/category mappings without parsing prose.
    public string Kind { get; set; } = "table";
    public string Title { get; set; } = "Results";
    public List<AnalyticsResultColumn> Columns { get; set; } = [];
    public List<JsonElement[]> Rows { get; set; } = [];
    public bool Truncated { get; set; }
}
public record AnalyticsResultColumn(string Key, string Label);
public sealed class AnalyticsSnapshot
{
    public string RefreshedAt { get; set; } = "";
    public string FirstDate { get; set; } = "";
    public string LastDate { get; set; } = "";
}
public sealed class AnalyticsChatStatus
{
    public bool Configured { get; set; }
    public AnalyticsSnapshot? Snapshot { get; set; }
}
