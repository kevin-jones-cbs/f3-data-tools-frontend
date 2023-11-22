using Blazorise;
using Blazorise.DataGrid;
using F3Core;
using F3Wasm.Data;
using F3Wasm.Models;


namespace F3Wasm.Pages
{
    public partial class Exicon
    {
        public static List<ExiconEntry> AllEntries { get; set; }
        public static List<ExiconEntry> CurrentEntries { get; set; }
        public string advancedSearch { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Deserialize the json string into an OgModel object. Using System.Text.Json
            AllEntries = ExiconData.Entries;
            CurrentEntries = AllEntries;
        }
        
        private async Task AdvancedSearch()
        {
            Console.WriteLine("Advanced Search: " + advancedSearch);

            var result = await LambdaHelper.SearchExiconEntriesAsync(Http, advancedSearch);
            CurrentEntries = result;
        }
        private async Task Reset()
        {
            CurrentEntries = AllEntries;
            advancedSearch = "";
        }
    }
}