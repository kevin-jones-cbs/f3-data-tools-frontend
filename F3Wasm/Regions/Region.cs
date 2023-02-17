using F3Wasm.Models;

namespace F3Wasm.Regions
{
    public abstract class Region
    {
        public abstract string QueryStringValue { get; }
        public abstract string DisplayName { get; }
        
        public abstract List<Ao> AoList { get; }
    }
}