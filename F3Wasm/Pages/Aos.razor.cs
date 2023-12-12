
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
using ApexCharts;

namespace F3Wasm.Pages
{
    public class AoDisplay
    {
        public Ao Ao { get; set; }
        public int Count { get; set; }
        public string HexColor { get; set; }
    }

    public partial class Aos
    {
        [Parameter]
        public string Region { get; set; }
        public Region RegionInfo { get; set; }

        public AllData allData { get; set; }
        public List<AoDisplay> aoCounts { get; private set; }
        private ApexChart<AoDisplay> chart;
        private ApexChartOptions<AoDisplay> options;

        Dropdown yearDropdown;
        Dropdown monthDropdown;
        public List<int> validYears { get; set; }
        public List<string> validMonths { get; set; } = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public int currentYear { get; set; }
        public string currentMonth { get; set; }

        public OverallView currentView { get; set; } = OverallView.AllTime;
        public SortView currentSort { get; set; } = SortView.DayOfWeek;

        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            allData = await LambdaHelper.GetAllDataAsync(Http, Region);

            validYears = allData.Posts.Select(p => p.Date.Year).Distinct().OrderByDescending(x => x).Where(x => x <= DateTime.Now.Year).ToList();
            validMonths = validMonths.OrderByDescending(x => DateTime.ParseExact(x, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month).ToList();

            await UpdateDisplay();

            options = new ApexChartOptions<AoDisplay>
            {
                Chart = new Chart
                {
                    Height = 400
                },
                PlotOptions = new PlotOptions
                {
                    Bar = new PlotOptionsBar
                    {
                        Horizontal = true
                    }
                },

            };
        }

        private async Task UpdateDisplay(OverallView overallView = OverallView.UNSET, int year = 0, string month = "")
        {
            // Handle currentViews
            if (overallView == OverallView.UNSET)
            {
                overallView = currentView;
            }
            else
            {
                currentView = overallView;
            }

            await Task.Delay(1);

            // Get all of the allData.Posts where the site exists in RegionInfo.AoList
            var validSites = RegionInfo.AoList.Select(x => x.Name).ToList();
            var allPosts = allData.Posts.Where(x => validSites.Contains(x.Site)).ToList();

            if (overallView == OverallView.Year)
            {
                currentYear = year;
                allPosts = allPosts.Where(x => x.Date.Year == year).ToList();
            }
            else if (overallView == OverallView.Month)
            {
                currentMonth = month;
                if (currentYear == 0)
                {
                    currentYear = DateTime.Now.Year;
                }

                var monthInt = DateTime.ParseExact(month, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                allPosts = allPosts.Where(x => x.Date.Year == currentYear && x.Date.Month == monthInt).ToList();                
            }

            // Group the posts by ao
            var aoGroups = allPosts.GroupBy(x => x.Site).ToList();

            // Dictionary of site name and count
            aoCounts = new List<AoDisplay>();
            foreach (var aoGroup in aoGroups)
            {
                var ao = RegionInfo.AoList.FirstOrDefault(x => x.Name == aoGroup.Key);
                if (ao != null)
                {
                    aoCounts.Add(new AoDisplay { Ao = ao, Count = aoGroup.Count(), HexColor = ColorHelpers.GetAoHex(ao) });
                }
            }

            aoCounts = aoCounts.OrderByDescending(x => x.Count).ToList();
        }

        private async Task Render()
        {
            await chart.RenderAsync();
        }
    }
}