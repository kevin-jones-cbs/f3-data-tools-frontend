using Blazorise;
using F3Core;
using F3Core.Regions;
using F3Wasm.Data;
using Microsoft.AspNetCore.Components;

namespace F3Wasm.Pages
{
    public partial class Index
    {
        [Parameter]
        public string Region { get; set; }
        public Region RegionInfo { get; set; }
        DatePicker<DateTime?> datePicker;
        public List<Ao> aoList { get; set; }

        private string comment = string.Empty;
        private List<string> allNames = new List<string>();
        private List<Pax> pax = new List<Pax>();
        public List<Ao> missingAos { get; set; } = new List<Ao>();
        public List<Ao> missingQSourceAos { get; set; } = new List<Ao>();
        private static DateTime? qDate = DateTime.Now;
        public string ao { get; set; }
        public static string AoOtherValue { get; } = "Other";
        public string otherAoName { get; set; }

        public string errorMessage { get; set; }
        public bool showNoMissingAoMessage { get; set; }
        public bool showCompleteAlert { get; set; }
        public bool isLoading { get; set; }
        public bool commentIsLoading { get; set; }
        public bool isMissingDataLoading { get; set; }
        public bool isQSource { get; set; }
        public bool isSiteClosed { get; set; }

        private List<DateTime> missingDates = new List<DateTime>();

        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            if (RegionInfo == null)
            {
                throw new Exception("Invalid Region");
            }

            aoList = await LambdaHelper.GetAllLocationsAsync(Http, Region);

            // Add Other to each day of week
            foreach (var day in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
            {
                aoList.Add(new Ao { Name = AoOtherValue, DayOfWeek = day, City = "Enter Details..." });
            }

            await OnMissingAoButtonClicked();
        }

        private string ShowOrHideAo(Ao ao)
        {
            return IsValidAo(ao) ? "Display: none" : string.Empty;
        }

        private bool IsValidAo(Ao ao)
        {
            return ao.DayOfWeek != qDate.Value.DayOfWeek;
        }

        private async Task OnMissingAoButtonClicked()
        {
            isMissingDataLoading = true;
            var allMissingAos = await LambdaHelper.GetMissingAosAsync(Http, Region);

            // Separate regular missing AOs from Q Source missing AOs based on HasQSource property
            missingAos = allMissingAos.Where(x => !x.HasQSource).ToList();
            missingQSourceAos = allMissingAos.Where(x => x.HasQSource).ToList();

            // Combine both lists to get all unique missing dates
            var allMissingDates = missingAos.Select(x => x.Date.Date)
                .Union(missingQSourceAos.Select(x => x.Date.Date))
                .Distinct()
                .ToList();
            missingDates = allMissingDates;

            if (!missingDates.Any())
            {
                showNoMissingAoMessage = true;
                await InvokeAsync(StateHasChanged);
            }
            isMissingDataLoading = false;
        }

        private async Task ClearCache()
        {
            isMissingDataLoading = true;
            await LambdaHelper.ClearCacheAsync(Http, Region);

            isMissingDataLoading = false;
        }

        private async Task OnMissingAoSelected(Ao missingAo, bool isQSourceMissing = false)
        {
            qDate = missingAo.Date;

            await OnAoChanged(missingAo.Name);

            // Set Q Source checkbox based on the type of missing entry
            isQSource = isQSourceMissing;

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnMissingDateSelected(DateTime date)
        {
            qDate = date;
            await OnDateChanged(date);
            await InvokeAsync(StateHasChanged);
        }

        private async Task OnDateChanged(DateTime? date)
        {
            if (qDate.Value.Date.Day != date.Value.Date.Day)
            {
                ao = string.Empty;
                qDate = date;
            }
        }

        private async Task OnAoChanged(string aoValue)
        {
            ao = aoValue;
            var selectedAo = aoList.FirstOrDefault(x => x.Name == aoValue);
            if (selectedAo != null && selectedAo.IsQSourceOnly)
            {
                isQSource = true;
            }
        }

        private async Task OnCommentButtonClicked()
        {
            commentIsLoading = true;
            allNames = await LambdaHelper.GetPaxNamesAsync(Http, Region);
            allNames = allNames.OrderBy(x => x).ToList();
            pax = await LambdaHelper.GetPaxFromCommentAsync(Http, Region, comment);

            // No need to show that we finished again, we're doing another ao
            showCompleteAlert = false;
            commentIsLoading = false;
        }

        private async Task OnAddRowClicked()
        {
            pax.Add(new Pax());
        }

        private async Task OnUploadButtonClicked()
        {
            // Validate
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(ao))
            {
                errorMessage = "Please select a valid AO";
                return;
            }

            if (qDate == null || !qDate.HasValue)
            {
                errorMessage = "Please select a Q Date";
                return;
            }

            // "Other" workouts don't need a Q
            if (!pax.Any(x => x.IsQ) && ao != AoOtherValue)
            {
                errorMessage = "Please select a Q";
                return;
            }

            if (pax.Any(x => string.IsNullOrEmpty(x.Name)))
            {
                errorMessage = "Please Fix the Missing Names";
                return;
            }

            if (pax.GroupBy(x => x.Name).Any(g => g.Count() > 1))
            {
                errorMessage = "Please Fix the Duplicate Names";
                return;
            }

            if (ao == AoOtherValue && string.IsNullOrEmpty(otherAoName))
            {
                errorMessage = "Please enter a name for the location";
                return;
            }

            if (pax.Any(x => x.IsDr && x.NamingRegionIndex == 0))
            {
                errorMessage = "Please Enter a Naming Region";
                return;
            }

            try
            {
                isLoading = true;
                await LambdaHelper.UploadPaxAsync(Http, Region, pax, ao == AoOtherValue ? otherAoName : ao, qDate.Value, isQSource);
                await ResetAfterUploadAsync();
                isLoading = false;
            }
            catch (Exception ex)
            {
                errorMessage = "There was an error uploading the data. Please try again." + ex.Message;
            }
        }

        private async Task OnSiteClosedClicked()
        {
            pax = new List<Pax> { new Pax { Name = "Site Closed (Archived)" } };
            await LambdaHelper.UploadPaxAsync(Http, Region, pax, ao == AoOtherValue ? otherAoName : ao, qDate.Value, isQSource);
            await ResetAfterUploadAsync();
        }

        private async Task ResetAfterUploadAsync()
        {
            showCompleteAlert = true;

            // Remove from missing list if it's there if ao and date match
            if (missingAos.Any(x => x.Name == ao && x.Date.Date == qDate.Value.Date))
            {
                missingAos.Remove(missingAos.First(x => x.Name == ao && x.Date.Date == qDate.Value.Date));
            }

            // Also remove from Q Source missing list if applicable
            if (missingQSourceAos.Any(x => x.Name == ao && x.Date.Date == qDate.Value.Date && isQSource))
            {
                missingQSourceAos.Remove(missingQSourceAos.First(x => x.Name == ao && x.Date.Date == qDate.Value.Date));
            }

            // If there are no more missing aos, show the message
            if (!missingAos.Any(x => x.Name != AoOtherValue) && !missingQSourceAos.Any())
            {
                showNoMissingAoMessage = true;
            }

            var allMissingDates = missingAos.Select(x => x.Date.Date)
                .Union(missingQSourceAos.Select(x => x.Date.Date))
                .Distinct()
                .ToList();
            missingDates = allMissingDates;


            // Reset everything
            comment = string.Empty;
            pax = new List<Pax>();
            qDate = DateTime.Now;
            ao = string.Empty;
            otherAoName = string.Empty;
            isQSource = false;
            isSiteClosed = false;

            //await OnMissingAoButtonClicked();
        }

        private string GetLocationCardClass(Ao ao)
        {
            if (missingAos.Any(x => x.Name == ao.Name && x.Date.Date == qDate.Value.Date))
            {
                return "location-card missing";
            }
            if (missingQSourceAos.Any(x => x.Name == ao.Name && x.Date.Date == qDate.Value.Date))
            {
                return "location-card missing-qsource";
            }
            return ao.Name == this.ao ? "location-card selected" : "location-card";
        }

        private string GetIndicatorClass(Ao ao)
        {
            if (missingAos.Any(x => x.Name == ao.Name && x.Date.Date == qDate.Value.Date))
            {
                return "indicator missing";
            }
            if (missingQSourceAos.Any(x => x.Name == ao.Name && x.Date.Date == qDate.Value.Date))
            {
                return "indicator missing-qsource";
            }
            return ao.Name == this.ao ? "indicator selected" : "indicator";
        }

        void OnQSelected(Pax member, bool isChecked)
        {
            //pax.ForEach(x => x.IsQ = false);
            if (isChecked)
            {
                member.IsQ = true;
            }
            else
            {
                member.IsQ = false;
            }
        }

        void OnFngSelected(Pax member, bool isChecked)
        {
            if (isChecked)
            {
                member.Name = member.UnknownName;
                member.UnknownName = string.Empty;
                member.IsFng = true;
            }
            else
            {
                member.UnknownName = member.Name;
                member.Name = string.Empty;
                member.IsFng = false;
            }
        }

        void OnRemovePax(Pax member)
        {
            pax.Remove(member);
        }
    }
}