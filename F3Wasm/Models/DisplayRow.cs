namespace F3Wasm.Models
{
    public class DisplayRow
    {
        public string PaxName { get; set; }
        public int PostCount { get; set; }
        public int QCount { get; set; }
        public DateTime? FirstPost { get; set; }
        public double PostPercent { get; set; }
        public int Streak { get; set; }
        public double QRatio { get; set; }

        // Kotter
        public DateTime? LastPost { get; set; }
        public DateTime? LastQ { get; set; }
        public int KotterDays { get; set; }

        // Ao Challenge
        public int AoPosts { get; set; }
        public double AoPercent { get; set; }
    }
}
