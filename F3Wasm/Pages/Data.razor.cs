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
using System.Web;
using Blazorise.DataGrid;

namespace F3Wasm.Pages
{
    public partial class Data
    {
        [Parameter]
        public string Region { get; set; }

        public Region RegionInfo { get; set; }
        public bool IsEmbed { get; set; }

        public AllData allData { get; set; }
        public OverallView currentView { get; set; }
        private DataGrid<DisplayRow> dataGrid;
        public List<DisplayRow> currentRows { get; set; }
        public DateTime lastUpdatedDate { get; set; }
        public bool showPaxModal { get; set; }
        public Pax selectedPax { get; set; }
        public List<Post> selectedPaxPosts { get; set; }
        public int selectedPax100Count { get; set; }
        public int selectedPaxPossibleCount { get; set; }
        public IReadOnlyList<DateTime?> selectedPaxDates { get; set; }
        public IReadOnlyList<DateTime?> disabledPaxQDates { get; set; }
        public string selectedPaxPostWithView { get; set; } = null;
        public Dictionary<string, int> selectedPaxPostedWith { get; set; }
        public DateTime selectedPaxStreakStart { get; set; }
        public int selectedPaxStreak { get; set; }
        public bool showOtherLocations { get; set; } = false;

        Dropdown yearDropdown;
        Dropdown monthDropdown;
        public List<int> validYears { get; set; }
        public List<string> validMonths { get; set; } = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public int currentYear { get; set; }
        public string currentMonth { get; set; }

        public List<WorkoutDay> AllPossibleWorkoutDays { get; set; }
        private string customFilterValue;

        public bool loading { get; set; }

        // Don't show the modal for these PAX
        private static List<string> OptedOutPax = new List<string>();
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            allData = await LambdaHelper.GetAllDataAsync(Http, Region);

            validYears = allData.Posts.Select(p => p.Date.Year).Distinct().OrderByDescending(x => x).Where(x => x <= DateTime.Now.Year).ToList();
            // Order the months by month number
            validMonths = validMonths.OrderByDescending(x => DateTime.ParseExact(x, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month).ToList();

            var lastUpdateItem = allData.Posts.FirstOrDefault(x => x.Site == "UPDATE");
            if (lastUpdateItem != null)
            {
                lastUpdatedDate = DateTime.Parse(lastUpdateItem.Pax.Split(" ")[1]);
                allData.Posts.Remove(lastUpdateItem);
            }

            AllPossibleWorkoutDays = GetCurrentPossibleWorkoutDays(allData.Posts);

            // Get IsEmbed from the query string
            var uri = new Uri(NavigationManager.Uri);
            var hasIsEmbedParam = HttpUtility.ParseQueryString(uri.Query).AllKeys.Contains("IsEmbed");

            if (hasIsEmbedParam)
            {
                var isEmbedParam = HttpUtility.ParseQueryString(uri.Query)["IsEmbed"];
                bool.TryParse(isEmbedParam, out var isEmbed);
                IsEmbed = isEmbed;
            }

            await ShowAllTime();
        }

        private async Task SetupDuplicateDates(string[] dates2, string[] dates3)
        {
            await JSRuntime.InvokeVoidAsync("setupDuplicateDates", dates2, dates3);
        }

        public void SetCurrentRows(List<Post> posts, DateTime? firstDay, DateTime lastDay)
        {
            var currentUniqueWorkoutDayCount = -1;
            if (firstDay != null)
            {
                currentUniqueWorkoutDayCount = AllPossibleWorkoutDays.Where(x => x.Date >= firstDay && x.Date <= lastDay).Count();
            }

            // Group the posts by pax name
            var paxPosts = posts.GroupBy(p => p.Pax).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Date).ToList());
            currentRows = new List<DisplayRow>();

            // Add the pax to the display
            foreach (var pax in paxPosts)
            {
                var row = new DisplayRow
                {
                    PaxName = pax.Key,
                    PostCount = pax.Value.Count,
                };

                var paxFirstDate = pax.Value.Min(p => p.Date);

                if (firstDay == null)
                {
                    currentUniqueWorkoutDayCount = AllPossibleWorkoutDays.Where(x => x.Date >= paxFirstDate).Count();
                }

                if (paxFirstDate != DateTime.MinValue)
                {
                    row.FirstPost = paxFirstDate;
                    row.PostPercent = (double)row.PostCount / (double)currentUniqueWorkoutDayCount * 100;
                }

                // Get the Q count
                row.QCount = pax.Value.Where(p => p.IsQ).Count();

                (var streak, var streakStart) = CalculateStreak(pax.Value, RegionInfo);
                row.Streak = streak;

                // Kotter
                if (currentView == OverallView.Kotter)
                {
                    row.LastPost = pax.Value.Max(p => p.Date);
                    row.KotterDays = (DateTime.Now - row.LastPost.Value).Days;
                }

                // Q Kotter
                if (currentView == OverallView.QKotter)
                {
                    row.LastPost = pax.Value.Where(x => x.IsQ).Max(p => p.Date);
                    row.KotterDays = (DateTime.Now - row.LastPost.Value).Days;
                }
                

                // Ao Challenge. Find the number of unique aos for this pax, for this month
                if (currentView == OverallView.AoChallenge || currentView == OverallView.AoList)
                {
                    row.AoPosts = pax.Value.GroupBy(p => p.Site).Select(g => g.Key).Count();
                    row.AoPercent = (double)row.AoPosts / RegionInfo.AoList.Count * 100;
                }

                currentRows.Add(row);
            }

            if (currentView == OverallView.Kotter || currentView == OverallView.QKotter)
            {
                currentRows = currentRows.OrderBy(r => r.KotterDays).ToList();
            }
            else if (currentView == OverallView.AoChallenge || currentView == OverallView.AoList)
            {
                currentRows = currentRows.OrderByDescending(r => r.AoPosts).ToList();
            }
            else
            {
                currentRows = currentRows.OrderByDescending(r => r.PostCount).ToList();
            }

            loading = false;
        }

        private static (int, DateTime) CalculateStreak(List<Post> posts, Region region)
        {
            int currentStreak = 0;
            int longestStreak = 0;
            DateTime? lastWorkoutDate = null;
            DateTime currentStreakStart = DateTime.MinValue;
            DateTime longestStreakStart = DateTime.MinValue;
            DateTime firstSundayOpp = new DateTime(2023, 4, 16);

            foreach (var workoutDay in posts)
            {
                if (lastWorkoutDate == null)
                {
                    currentStreak = 1;
                    currentStreakStart = workoutDay.Date;
                }
                else if (workoutDay.Date == lastWorkoutDate.Value.AddDays(0))
                {
                    // Do nothing. Duplicate day.
                }
                else if (workoutDay.Date == lastWorkoutDate.Value.AddDays(1) ||
                    (workoutDay.Date.DayOfWeek == DayOfWeek.Monday && (workoutDay.Date < firstSundayOpp || region.DisplayName == "Asgard" || region.DisplayName == "Delta") && workoutDay.Date == lastWorkoutDate.Value.AddDays(2))) // Handle before we had Sundays
                {
                    currentStreak++;
                }
                else
                {
                    // If the dates are not consecutive, reset the streak
                    currentStreak = 1;
                    currentStreakStart = workoutDay.Date;
                }

                longestStreak = Math.Max(longestStreak, currentStreak);
                if (longestStreak == currentStreak)
                {
                    longestStreakStart = currentStreakStart;
                }

                lastWorkoutDate = workoutDay.Date;
            }

            return (longestStreak, longestStreakStart);
        }

        public static List<WorkoutDay> GetCurrentPossibleWorkoutDays(List<Post> posts)
        {
            var currentUniqueWorkoutDays = new List<WorkoutDay>();
            foreach (var post in posts)
            {
                DateTime postDateWithoutTime = post.Date.Date;
                bool isEvening = post.Site.Contains("moon", StringComparison.OrdinalIgnoreCase);
                var key = $"{postDateWithoutTime.ToShortDateString()}-{isEvening}";

                // Check if the day and isEvening is already in the list
                if (currentUniqueWorkoutDays.Any(x => x.Date == postDateWithoutTime && x.IsEvening == isEvening))
                {
                    continue;
                }

                currentUniqueWorkoutDays.Add(new WorkoutDay() { Date = postDateWithoutTime, IsEvening = isEvening });
            }

            return currentUniqueWorkoutDays;
        }

        #region Show AllTime, Years, Month, etc.
        private async Task ShowAllTime()
        {
            await Task.Delay(1);
            loading = true;
            currentView = OverallView.AllTime;
            SetCurrentRows(allData.Posts, null, DateTime.Now);

            await RefreshDropdowns();
        }

        private async Task ShowYear(int year)
        {
            await Task.Delay(1);
            currentView = OverallView.Year;
            currentYear = year;
            var posts = allData.Posts.Where(p => p.Date.Year == year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year - 1, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 1, 1).AddDays(-1);
            SetCurrentRows(posts, firstDay, lastDay);
            await RefreshDropdowns();
        }

        private async Task ShowMonth(string month)
        {
            loading = true;
            currentView = OverallView.Month;
            currentMonth = month;
            if (currentYear == 0)
            {
                currentYear = DateTime.Now.Year;
            }

            var monthInt = DateTime.ParseExact(month, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;

            var posts = allData.Posts.Where(p => p.Date.Month == monthInt && p.Date.Year == currentYear).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
            SetCurrentRows(posts, firstDay, lastDay);
            await RefreshDropdowns();
        }

        private async Task ShowKotter()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.Kotter;
            var paxLastPostDates = allData.Posts.GroupBy(p => p.Pax).Select(g => new { Name = g.Key, MaxDate = g.Max(x => x.Date), Region = allData.Pax.FirstOrDefault(p => p.Name == g.Key)?.NamingRegion }).ToList();

            // Only show pax who haven't posted in the last 14 days, less than a year ago, where this is their home region.
            var posts = allData.Posts.Where(p => paxLastPostDates.Any(x =>
                                            x.Name == p.Pax &&
                                            !p.Pax.Contains("2.0") &&
                                            x.MaxDate < DateTime.Now.AddDays(-14) &&
                                            x.MaxDate > DateTime.Now.AddDays(-365) &&
                                            x.Region == RegionInfo.DisplayName)).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);
            await RefreshDropdowns();
        }

        private async Task ShowQKotter()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.QKotter;
            var paxLastQPostDates = allData.Posts.Where(p => p.IsQ).GroupBy(p => p.Pax).Select(g => new { Name = g.Key, MaxDate = g.Max(x => x.Date), Region = allData.Pax.FirstOrDefault(p => p.Name == g.Key)?.NamingRegion }).ToList();

            // Only show pax who haven't posted in the last 14 days, less than a year ago, where this is their home region.
            var posts = allData.Posts.Where(p => paxLastQPostDates.Any(x =>
                                            x.Name == p.Pax &&
                                            x.MaxDate < DateTime.Now.AddDays(-14) &&
                                            x.MaxDate > DateTime.Now.AddDays(-365))).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);
            await RefreshDropdowns();
        }

        private async Task ShowAoChallenge()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.AoChallenge;

            var posts = allData.Posts.Where(p => p.Date.Month == 11 && p.Date.Year == DateTime.Now.Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 11, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);

            loading = false;
            await RefreshDropdowns();
        }

        private async Task ShowAoList()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.AoList;

            var posts = allData.Posts.Where(p => p.Date.Month == DateTime.Now.Month && p.Date.Year == DateTime.Now.Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay);

            loading = false;
            await RefreshDropdowns();
        }


        private async Task RefreshDropdowns()
        {
            await Task.Delay(1);
            if (yearDropdown != null && monthDropdown != null)
            {
                await yearDropdown.Show();
                await yearDropdown.Hide();
                await monthDropdown.Show();
                await monthDropdown.Hide();
            }
        }

        #endregion

        // Filter
        private Task OnCustomFilterValueChanged(string e)
        {
            customFilterValue = e;
            return dataGrid.Reload();
        }

        private bool OnCustomFilter(DisplayRow model)
        {
            if (string.IsNullOrEmpty(customFilterValue))
                return true;

            return model.PaxName?.Contains(customFilterValue, StringComparison.OrdinalIgnoreCase) == true;
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

        private Task OnModalClosing(ModalClosingEventArgs e)
        {
            selectedPax = null;

            return Task.CompletedTask;
        }

        private async Task SelectedRowChanged(DisplayRow row)
        {
            if (OptedOutPax.Contains(row.PaxName))
            {
                // Don't show modal for opted out pax
                return;
            }

            selectedPaxPosts = allData.Posts.Where(p => p.Pax == row.PaxName).OrderByDescending(x => x.Date).ToList();
            selectedPaxDates = selectedPaxPosts.Select(p => (DateTime?)p.Date).ToList();
            disabledPaxQDates = selectedPaxPosts.Where(p => p.IsQ).Select(p => (DateTime?)p.Date).ToList();
            selectedPaxPostedWith = new Dictionary<string, int>();
            selectedPax100Count = selectedPaxPosts.Count / 100;
            selectedPaxPossibleCount = AllPossibleWorkoutDays.Where(x => x.Date >= selectedPaxPosts.LastOrDefault().Date).Count();
            selectedPaxPostWithView = null;
            selectedPax = allData.Pax.FirstOrDefault(p => p.Name == row.PaxName);

            // Get the dates that were posted two and three times
            var dates2 = selectedPaxPosts.GroupBy(p => p.Date).Where(g => g.Count() == 2).Select(g => g.Key.ToString("MMMM d, yyyy")).ToArray();
            var dates3 = selectedPaxPosts.GroupBy(p => p.Date).Where(g => g.Count() == 3).Select(g => g.Key.ToString("MMMM d, yyyy")).ToArray();

            var paxPostsAsc = selectedPaxPosts.OrderBy(p => p.Date).ToList();
            (selectedPaxStreak, selectedPaxStreakStart) = CalculateStreak(paxPostsAsc, RegionInfo);

            showOtherLocations = false;

            await ShowModal();
            await SetupDuplicateDates(dates2, dates3);
        }

        private Task OnDatesChanged(IReadOnlyList<DateTime?> date)
        {
            return Task.CompletedTask;
        }

        private string GetPaxLocationColor(Ao location)
        {
            string hex = ColorHelpers.GetAoHex(location);

            return $"background-color: {hex};";
        }

        public string FullMoonRuckName = "Full Moon Ruck";
        public Ao FullMoonRuckAo = new Ao() { Name = "Full Moon Ruck" };
        private string GetAoChallengeButtonColor(Ao location)
        {
            string hex;

            if (location.Name == FullMoonRuckName)
            {
                hex = "#423c3f";
            }
            else
            {
                hex = ColorHelpers.GetAoHex(location);
            }

            var now = DateTime.Now;
            var hasPosted = selectedPaxPosts.Where(x => x.Date.Year == now.Year && x.Date.Month == (currentView == OverallView.AoChallenge ? 11 : now.Month)).Any(x => x.Site == location.Name);

            if (hasPosted)
            {
                return $"background-color: {hex}; color: white;";
            }

            return $"border: solid 1px {hex}; color:{hex}";
        }

        private async Task OnSelectedPaxPostWithViewChange(string index)
        {
            selectedPaxPostedWith = GetAllTimePaxPostWith(index, selectedPaxPosts, allData, selectedPax100Count);
            selectedPaxPostWithView = index;
        }

        private async Task OnShowOtherLocationsClicked()
        {
            showOtherLocations = !showOtherLocations;
        }

        public string GetNamedBy()
        {
            var paxFirstPost = selectedPaxPosts.LastOrDefault();
            if (paxFirstPost == null)
                return "";

            var namedBy = allData.Posts.Where(p => p.Site == paxFirstPost.Site && p.IsQ && p.Date == paxFirstPost.Date).FirstOrDefault();

            if (namedBy == null)
                return "";

            return namedBy.Pax;
        }

        public string GetDaysTo100()
        {
            // Get the 100th post for the selected pax
            var pax100thPost = selectedPaxPosts.OrderBy(x => x.Date).Skip(99).FirstOrDefault();
            var notYet = false;
            if (pax100thPost == null)
            {
                // No 100 yet, so just count today
                pax100thPost = new Post() { Date = DateTime.Now };
                notYet = true;
            }

            var pax1stPost = selectedPaxPosts.LastOrDefault();

            // Get the number of days since their first post
            var days = AllPossibleWorkoutDays.Where(x => x.Date >= pax1stPost.Date && x.Date <= pax100thPost.Date).ToList();
            return days.Count.ToString() + (notYet ? " (so far)" : "");
        }

        public string GetCalDaysTo100()
        {
            // Get the 100th post for the selected pax
            var pax100thPost = selectedPaxPosts.OrderBy(x => x.Date).Skip(99).FirstOrDefault();
            var notYet = false;
            if (pax100thPost == null)
            {
                // No 100 yet, so just count today
                pax100thPost = new Post() { Date = DateTime.Now };
                notYet = true;
            }

            var pax1stPost = selectedPaxPosts.LastOrDefault();

            // Get the number of days between their first post and 100th post
            var days = (pax100thPost.Date - pax1stPost.Date).Days;

            // Get the number of days since their first post
            return days.ToString() + (notYet ? " (so far)" : "");
        }

        public static Dictionary<string, int> GetAllTimePaxPostWith(string index, List<Post> selectedPaxPosts, AllData allData, int selectedPax100Count)
        {
            List<Post> paxPosts = new List<Post>();
            var newPaxPostedWith = new Dictionary<string, int>();

            // All Time
            if (index == "AllTime" || index == "QAllTime")
            {
                paxPosts = selectedPaxPosts.OrderBy(p => p.Date).ToList();
            }
            else if (index == "Recent100")
            {
                paxPosts = selectedPaxPosts.OrderBy(p => p.Date).Skip(selectedPax100Count * 100).ToList();
            }
            else if (index == "QOthersAllTime")
            {
                paxPosts = selectedPaxPosts.Where(x => x.IsQ).OrderBy(p => p.Date).ToList();
            }
            else
            {
                var intIndex = int.Parse(index);
                paxPosts = selectedPaxPosts.OrderBy(p => p.Date).Skip(intIndex * 100).Take(intIndex + 1 * 100).ToList();
            }

            foreach (var paxPost in paxPosts)
            {
                var matched = allData.Posts.Where(p => p.Date == paxPost.Date && p.Site == paxPost.Site && p.Pax != paxPost.Pax).ToList();
                if (index == "QAllTime")
                {
                    matched = matched.Where(p => p.IsQ).ToList();
                }

                foreach (var pax in matched)
                {
                    if (newPaxPostedWith.ContainsKey(pax.Pax))
                    {
                        newPaxPostedWith[pax.Pax]++;
                    }
                    else
                    {
                        newPaxPostedWith.Add(pax.Pax, 1);
                    }
                }
            }

            return newPaxPostedWith.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}