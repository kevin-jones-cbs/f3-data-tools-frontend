using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using F3Wasm;
using Blazorise;
using Blazorise.Components;
using F3Wasm.Models;
using F3Wasm.Data;
using F3Core;
using F3Core.Regions;

namespace F3Wasm.Pages
{
    public partial class Index
    {
        [Parameter]
        public string Region { get; set; }
        public Region RegionInfo { get; set; }

        private string comment = string.Empty;
        private List<string> allNames = new List<string>();
        private List<Pax> pax = new List<Pax>();
        public List<Ao> missingAos { get; set; } = new List<Ao>();
        private static DateTime? qDate = DateTime.Now;
        public string ao { get; set; }
        public static string AoOtherValue { get; } = "other";
        public string otherAoName { get; set; }

        public string errorMessage { get; set; }
        public bool showNoMissingAoMessage { get; set; }
        public bool showCompleteAlert { get; set; }
        public bool isLoading { get; set; }
        public bool isMissingDataLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            if (RegionInfo == null)
            {
                throw new Exception("Invalid Region");
            }
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
            missingAos = await LambdaHelper.GetMissingAosAsync(Http, Region);
            if (!missingAos.Any())
            {
                showNoMissingAoMessage = true;
            }

            isMissingDataLoading = false;
        }

        private async Task OnMissingAoSelected(Ao missingAo)
        {
            qDate = missingAo.Date;
            await Task.Delay(50);
            ao = missingAo.Name;

            Console.WriteLine($"Selected {missingAo.Name} on {missingAo.Date}. ao is now {ao}");
        }

        private async Task OnDateChanged(DateTime? date)
        {
            qDate = date;
            ao = string.Empty;
        }

        private async Task OnAoChanged(string aoValue)
        {
            ao = aoValue;
        }

        private async Task OnCommentButtonClicked()
        {
            allNames = await LambdaHelper.GetPaxNamesAsync(Http, Region);
            allNames = allNames.OrderBy(x => x).ToList();
            pax = PaxHelper.GetPaxFromComment(comment, allNames);

            // No need to show that we finished again, we're doing another aoo
            showCompleteAlert = false;
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
                await LambdaHelper.UploadPaxAsync(Http, Region, pax, ao == AoOtherValue ? otherAoName : ao, qDate.Value);
                showCompleteAlert = true;

                // Remove from missing list if it's there if ao and date match
                if (missingAos.Any(x => x.Name == ao && x.Date.Date == qDate.Value.Date))
                {
                    missingAos.Remove(missingAos.First(x => x.Name == ao && x.Date.Date == qDate.Value.Date));

                    // If there are no more missing aos, show the message
                    if (!missingAos.Any())
                    {
                        showNoMissingAoMessage = true;
                    }
                }

                // Reset everything
                comment = string.Empty;
                pax = new List<Pax>();
                qDate = DateTime.Now;
                ao = string.Empty;
                otherAoName = string.Empty;
                isLoading = false;
            }
            catch (Exception ex)
            {
                errorMessage = "There was an error uploading the data. Please try again." + ex.Message;
            }
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