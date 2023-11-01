
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

        public OverallView currentView { get; set; } = OverallView.AllTime;
        public SortView currentSort { get; set; } = SortView.DayOfWeek;

        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            allData = await LambdaHelper.GetAllDataAsync(Http, Region);

            UpdateDisplay();

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

        private void UpdateDisplay(OverallView overallView = OverallView.UNSET, SortView sortView = SortView.UNSET)
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

            if (sortView == SortView.UNSET)
            {
                sortView = currentSort;
            }
            else
            {
                currentSort = sortView;
            }

            // Get all of the allData.Posts where the site exists in RegionInfo.AoList
            var validSites = RegionInfo.AoList.Select(x => x.Name).ToList();
            var allPosts = allData.Posts.Where(x => validSites.Contains(x.Site)).ToList();

            switch (overallView)
            {
                case OverallView.AllTime:
                    break;
                // case OverallView.LastYear:
                //     allPosts = allPosts.Where(x => x.Date >= new DateTime(DateTime.Now.Year - 1, 1, 1) && x.Date <= new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1)).ToList();
                //     break;
                // case OverallView.ThisYear:
                //     allPosts = allPosts.Where(x => x.Date >= new DateTime(DateTime.Now.Year, 1, 1)).ToList();
                //     break;
                // case OverallView.LastMonth:
                //     allPosts = allPosts.Where(x => x.Date >= new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1) && x.Date <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1)).ToList();
                //     break;
                // case OverallView.ThisMonth:
                //     allPosts = allPosts.Where(x => x.Date >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).ToList();
                //     break;
                default:
                    break;
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

            // Sort by count
            if (sortView == SortView.Count)
            {
                aoCounts = aoCounts.OrderByDescending(x => x.Count).ToList();
            }
            else
            {
                aoCounts = aoCounts.OrderBy(x => x.Ao.DayOfWeek).ToList();
            }
        }

        private async Task Render()
        {
            await chart.RenderAsync();
        }
    }
}