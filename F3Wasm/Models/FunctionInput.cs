namespace F3Wasm.Models
{
    public class FunctionInput
    {
        public string Action { get; set; }
        public List<Pax> Pax { get; set; }
        public string AoName { get; set; }
        public DateTime QDate { get; set; }
        public bool IsTesting { get; set; } 
    }
}
