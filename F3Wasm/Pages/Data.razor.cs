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

namespace F3Wasm.Pages
{
    public partial class Data
    {
        public AllData allData { get; set; }
        public OverallView currentView { get; set; }
        public List<DisplayRow> currentRows { get; set; }
        public DateTime lastUpdatedDate { get; set; }
        public bool showPaxModal { get; set; }
        public Pax selectedPax { get; set; }
        public List<Post> selectedPaxPosts { get; set; }
        public int selectedPax100Count { get; set; }
        public IReadOnlyList<DateTime?> selectedPaxDates { get; set; }
        public IReadOnlyList<DateTime?> disabledPaxQDates { get; set; }
        public string selectedPaxPostWithView { get; set; } = null;
        public Dictionary<string, int> selectedPaxPostedWith { get; set; }

        protected override async Task OnInitializedAsync()
        {
            allData = await LambdaHelper.GetAllDataAsync(Http);
            var lastUpdateItem = allData.Posts.FirstOrDefault(x => x.Site == "UPDATE");
            if (lastUpdateItem != null)
            {
                lastUpdatedDate = DateTime.Parse(lastUpdateItem.Pax.Split(" ")[1]);
                allData.Posts.Remove(lastUpdateItem);
            }

            ShowAllTime();
        }

        public void SetCurrentRows(List<Post> posts, DateTime? firstDay, DateTime lastDay)
        {
            // Group the posts by pax name
            var paxPosts = posts.GroupBy(p => p.Pax).ToDictionary(g => g.Key, g => g.Count());
            currentRows = new List<DisplayRow>();

            // Add the pax to the display
            foreach (var pax in paxPosts)
            {
                var row = new DisplayRow
                {
                    PaxName = pax.Key,
                    PostCount = pax.Value
                };

                var paxFirstDate = posts.Where(p => p.Pax == pax.Key).Min(p => p.Date);
                if (paxFirstDate != DateTime.MinValue)
                {
                    row.FirstPost = paxFirstDate;
                    row.PostPercent = (double)row.PostCount / (double)GetDaysSince(lastDay, firstDay ?? paxFirstDate) * 100;
                }

                // Get the Q count
                row.QCount = posts.Where(p => p.Pax == pax.Key && p.IsQ).Count();

                currentRows.Add(row);
            }

            currentRows = currentRows.OrderByDescending(r => r.PostCount).ToList();
        }

        public static int GetDaysSince(DateTime today, DateTime date)
        {
            try
            {
                if (date == null)
                    return -1;

                // Return the number of days since the date
                var days = (today - date).Days + 1;

                // Return the number of sundays since the date
                var sundays = Enumerable.Range(0, days + 1).Count(d => date.AddDays(d).DayOfWeek == DayOfWeek.Sunday);

                return days - sundays;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Error in GetDaysSince" + date.ToString() + " " + today.ToString());
                throw;
            }
        }

        private void ShowAllTime()
        {
            currentView = OverallView.AllTime;
            SetCurrentRows(allData.Posts, null, DateTime.Now);
        }

        private void ShowPreviousMonth()
        {
            currentView = OverallView.LastMonth;
            var posts = allData.Posts.Where(p => p.Date.Month == DateTime.Now.AddMonths(-1).Month && p.Date.Year == DateTime.Now.AddMonths(-1).Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            SetCurrentRows(posts, firstDay, lastDay);
        }

        private void ShowCurrentMonth()
        {
            currentView = OverallView.ThisMonth;
            var posts = allData.Posts.Where(p => p.Date.Month == DateTime.Now.Month && p.Date.Year == DateTime.Now.Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);
        }

        private void ShowPreviousYear()
        {
            currentView = OverallView.LastYear;
            var posts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.AddYears(-1).Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year - 1, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1);
            SetCurrentRows(posts, firstDay, lastDay);
        }

        private void ShowCurrentYear()
        {
            currentView = OverallView.ThisYear;
            var posts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);
        }

        // Modals
        private Task ShowModal()
        {
            showPaxModal = true;

            return Task.CompletedTask;
        }

        private Task HideModal()
        {
            showPaxModal = false;

            return Task.CompletedTask;
        }

        private Task SelectedRowChanged(DisplayRow row)
        {
            Console.WriteLine("SelectedRowChanged " + row.PaxName);

            selectedPax = allData.Pax.FirstOrDefault(p => p.Name == row.PaxName);
            selectedPaxPosts = allData.Posts.Where(p => p.Pax == row.PaxName).ToList();
            selectedPaxDates = selectedPaxPosts.Select(p => (DateTime?)p.Date).OrderByDescending(x => x).ToList();
            disabledPaxQDates = selectedPaxPosts.Where(p => p.IsQ).Select(p => (DateTime?)p.Date).OrderByDescending(x => x).ToList();
            selectedPaxPostedWith = new Dictionary<string, int>();
            selectedPax100Count = selectedPaxPosts.Count / 100;
            selectedPaxPostWithView = "AllTime";
            OnSelectedPaxPostWithViewChange(selectedPaxPostWithView);
            ShowModal();

            Console.WriteLine(selectedPaxPosts.Count);
            Console.WriteLine(selectedPaxPosts.Count / 100);

            return Task.CompletedTask;
        }

        private Task OnDatesChanged(IReadOnlyList<DateTime?> date)
        {
            Console.WriteLine("OnDateChanged " + date.ToString());
            return Task.CompletedTask;
        }

        private string GetPaxLocationColor(Ao location)
        {
            // Color white
            string hex;
            switch (location.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    hex = "#ff5b72";
                    break;
                case DayOfWeek.Tuesday:
                    hex = "#9370f0";
                    break;
                case DayOfWeek.Wednesday:
                    hex = "#32b0e5";
                    break;
                case DayOfWeek.Thursday:
                    hex = "#fdb00d";
                    break;
                case DayOfWeek.Friday:
                    hex = "#00b994";
                    break;
                case DayOfWeek.Saturday:
                    hex = "#25b808";                    
                    break;
                default:
                    hex = "#ffffff";
                    break;
            }

            return $"background-color: {hex};";
        }

        private void OnSelectedPaxPostWithViewChange(string index)
        {
            Console.WriteLine("OnSelectedPaxPostWithViewChange " + index);
            GetAllTimePaxPostWith(index);
        }
        private void GetAllTimePaxPostWith(string index)
        {
            List<Post> paxPosts = new List<Post>();
            selectedPaxPostedWith = new Dictionary<string, int>();
            selectedPaxPostWithView = index;

            // All Time
            if (index == "AllTime")
            {
                paxPosts = selectedPaxPosts.OrderByDescending(p => p.Date).ToList();
            }
            else if (index == "Recent100")
            {
                paxPosts = selectedPaxPosts.OrderByDescending(p => p.Date).Skip(selectedPax100Count * 100).ToList();
            }
            else 
            {
                var intIndex = int.Parse(index);
                paxPosts = selectedPaxPosts.OrderByDescending(p => p.Date).Skip(intIndex * 100).Take(intIndex + 1 * 100).ToList();
            }

            foreach (var paxPost in paxPosts)
            {
                var matched = allData.Posts.Where(p => p.Date == paxPost.Date && p.Site == paxPost.Site && p.Pax != paxPost.Pax).ToList();
                foreach (var pax in matched)
                {
                    if (selectedPaxPostedWith.ContainsKey(pax.Pax))
                    {
                        selectedPaxPostedWith[pax.Pax]++;
                    }
                    else
                    {
                        selectedPaxPostedWith.Add(pax.Pax, 1);
                    }
                }
            }

            selectedPaxPostedWith = selectedPaxPostedWith.OrderByDescending(p => p.Value).Take(10).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}