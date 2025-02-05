using Blazorise;

namespace F3Wasm.Models
{
    public class OverallStat
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public IconName Icon { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
