namespace F3Wasm.Models
{
    public class CurrentDisplay
    {
        public List<string> Columns { get; set; }
        public List<DisplayRow> Rows { get; set; }
    }

    public class DisplayRow
    {
        public string PaxName { get; set; }
        public int PostCount { get; set; }
        public DateTime? FirstPost { get; set; }
        public double PostPercent { get; set; }
    }
}
