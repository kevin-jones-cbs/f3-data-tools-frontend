using Blazorise;
using Blazorise.DataGrid;
using F3Core;
using F3Core.Regions;
using F3Wasm.Data;
using F3Wasm.Helpers;
using F3Wasm.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Web;

namespace F3Wasm.Pages
{
    public partial class Data
    {
        [Parameter]
        public string Region { get; set; }

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        public Region RegionInfo { get; set; }
        public bool IsEmbed { get; set; }

        public AllData allData { get; set; }
        public DateTime? firstNonHistoricalDate { get; set; } // for regions with historical data, this is the first date available for processing
        public OverallView currentView { get; set; }
        private DataGrid<DisplayRow> dataGrid;
        public List<DisplayRow> currentRows { get; set; }
        public DateTime lastUpdatedDate { get; set; }
        public bool showPaxModal { get; set; }
        public bool showAoChallengeModal { get; set; }
        public Pax selectedPax { get; set; }
        public List<Post> selectedPaxPosts { get; set; }
        public List<Post> selectedPaxQSourcePosts { get; set; }
        public HistoricalData selectedPaxHistoricalData { get; set; }

        Dropdown yearDropdown;
        Dropdown monthDropdown;
        public List<int> validYears { get; set; }
        public List<string> validMonths { get; set; } = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        public int currentYear { get; set; }
        public string currentMonth { get; set; }

        public List<WorkoutDay> allPossibleWorkoutDays { get; set; }
        private string customFilterValue;

        public bool loading { get; set; }

        // Don't show the modal for these PAX
        private static List<string> OptedOutPax = new List<string>();


        protected override async Task OnInitializedAsync()
        {
            RegionInfo = RegionList.All.FirstOrDefault(x => x.QueryStringValue == Region);
            allData = await LambdaHelper.GetAllDataAsync(Http, Region);

            if (RegionInfo.HasHistoricalData)
            {
                firstNonHistoricalDate = allData.Posts.Min(x => x.Date);
            }

            validYears = allData.Posts.Select(p => p.Date.Year).Distinct().OrderByDescending(x => x).Where(x => x <= DateTime.Now.Year).ToList();
            // Order the months by month number
            validMonths = validMonths.OrderByDescending(x => DateTime.ParseExact(x, "MMM", System.Globalization.CultureInfo.InvariantCulture).Month).ToList();

            lastUpdatedDate = allData.Posts.Where(x => x.Date.Year <= DateTime.Now.AddYears(1).Year).Max(x => x.Date);

            var lastUpdateItem = allData.Posts.FirstOrDefault(x => x.Site == "UPDATE");

            // Legacy handling of the UPDATE post for last updated date.
            if (lastUpdateItem != null)
            {
                allData.Posts.Remove(lastUpdateItem);
            }

            allPossibleWorkoutDays = GetCurrentPossibleWorkoutDays(allData.Posts);

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

        public void SetCurrentRows(List<Post> posts, DateTime? firstDay, DateTime lastDay, bool combineWithHistorical)
        {
            var currentUniqueWorkoutDayCount = -1;
            if (firstDay != null)
            {
                currentUniqueWorkoutDayCount = allPossibleWorkoutDays.Where(x => x.Date >= firstDay && x.Date <= lastDay).Count();
            }

            // Group the posts by pax name
            var paxPosts = posts.GroupBy(p => p.Pax).ToDictionary(g => g.Key, g => g.OrderBy(x => x.Date).ToList());
            currentRows = new List<DisplayRow>();

            // Add the pax to the display
            foreach (var pax in paxPosts)
            {
                var historicalPosts = 0;
                var historicalQs = 0;
                var historicalFirstDate = (DateTime?)null;

                if (RegionInfo.HasHistoricalData && combineWithHistorical)
                {
                    var matchingHistoricalPost = allData.HistoricalData.FirstOrDefault(x => x.PaxName == pax.Key);
                    if (matchingHistoricalPost != null)
                    {
                        historicalPosts = matchingHistoricalPost.PostCount;
                        historicalQs = matchingHistoricalPost.QCount;
                        historicalFirstDate = matchingHistoricalPost.FirstPost;
                    }
                }

                var paxPostsWithData = pax.Value.Count;

                var row = new DisplayRow
                {
                    PaxName = pax.Key,
                    PostCount = paxPostsWithData + historicalPosts,
                };

                var paxFirstDate = pax.Value.Min(p => p.Date);

                if (firstDay == null)
                {
                    currentUniqueWorkoutDayCount = allPossibleWorkoutDays.Where(x => x.Date >= paxFirstDate).Count();
                }

                if (paxFirstDate != DateTime.MinValue)
                {
                    row.FirstPost = historicalFirstDate ?? paxFirstDate;
                    row.PostPercent = (double)paxPostsWithData / (double)currentUniqueWorkoutDayCount * 100;
                }

                // Get the Q count
                row.QCount = pax.Value.Where(p => p.IsQ).Count() + historicalQs;

                (var streak, var streakStart) = StreakHelpers.CalculateStreak(pax.Value, RegionInfo);
                row.Streak = streak;

                row.QRatio = row.QCount == 0 ? 0 : (double)row.QCount / row.PostCount * 100;

                if (RegionInfo.HasExtraActivity)
                {
                    row.ExtraActivityCount = pax.Value.Count(p => p.ExtraActivity);
                }

                // Kotter
                if (currentView == OverallView.Kotter)
                {
                    row.LastPost = pax.Value.Max(p => p.Date);
                    row.KotterDays = (DateTime.Now - row.LastPost.Value).Days;
                }

                // Q Kotter
                if (currentView == OverallView.QKotter)
                {
                    row.LastPost = pax.Value.Max(p => p.Date);
                    row.LastQ = pax.Value.Where(x => x.IsQ).Max(p => p.Date);
                    row.KotterDays = (DateTime.Now - row.LastQ.Value).Days;
                }

                // Ao Challenge. Find the number of unique aos for this pax, for this month
                if (currentView == OverallView.AoChallenge || currentView == OverallView.AoList)
                {
                    row.AoPosts = pax.Value.GroupBy(p => p.Site).Select(g => g.Key).Count();
                    row.AoPercent = (double)row.AoPosts / allData.Aos.Count * 100;
                }

                if (currentView == OverallView.AllTime)
                {
                    row.CalendarDaysTo100 = GetCalDaysTo100(pax.Value);
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

        public int? GetCalDaysTo100(List<Post> selectedPaxPosts)
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

            var pax1stPost = selectedPaxPosts.FirstOrDefault();

            // Get the number of days between their first post and 100th post
            var days = (pax100thPost.Date - pax1stPost.Date).Days;

            // Get the number of days since their first post
            return notYet ? 9999 : days;
        }

        public static List<WorkoutDay> GetCurrentPossibleWorkoutDays(List<Post> posts)
        {
            var currentUniqueWorkoutDays = new List<WorkoutDay>();
            foreach (var post in posts)
            {
                DateTime postDateWithoutTime = post.Date.Date;
                //bool isEvening = post.Site.Contains("moon", StringComparison.OrdinalIgnoreCase);
                var key = $"{postDateWithoutTime.ToShortDateString()}";

                // Check if the day and isEvening is already in the list
                if (currentUniqueWorkoutDays.Any(x => x.Date == postDateWithoutTime))
                {
                    continue;
                }

                currentUniqueWorkoutDays.Add(new WorkoutDay() { Date = postDateWithoutTime });
            }

            return currentUniqueWorkoutDays;
        }

        #region Show AllTime, Years, Month, etc.
        private async Task ShowAllTime()
        {
            await Task.Delay(1);
            loading = true;
            currentView = OverallView.AllTime;
            SetCurrentRows(allData.Posts, null, DateTime.Now, true);

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
            SetCurrentRows(posts, firstDay, lastDay, false);
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

            var firstDay = new DateTime(currentYear, monthInt, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            SetCurrentRows(posts, firstDay, lastDay, false);
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
                        (RegionInfo.RosterSheetColumns.Contains(RosterSheetColumn.NamingRegionName) ? x.Region == RegionInfo.DisplayName : 
                         RegionInfo.RosterSheetColumns.Contains(RosterSheetColumn.NamingRegionYN) ? x.Region == "N" :
                         true) 
                           )).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay, true);
            await RefreshDropdowns();
        }

        private async Task ShowQKotter()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.QKotter;
            var paxLastQPostDates = allData.Posts.Where(p => p.IsQ).GroupBy(p => p.Pax).Select(g => new { Name = g.Key, MaxDate = g.Max(x => x.Date), Region = allData.Pax.FirstOrDefault(p => p.Name == g.Key)?.NamingRegion }).ToList();

            // Only show pax who haven't posted in the last 30 days, less than a year ago, where this is their home region.
            var posts = allData.Posts.Where(p => paxLastQPostDates.Any(x =>
                                            x.Name == p.Pax &&
                                            x.MaxDate < DateTime.Now.AddDays(-30) &&
                                            x.MaxDate > DateTime.Now.AddDays(-365))).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, firstDay, lastDay, true);
            await RefreshDropdowns();
        }

        private async Task ShowAoChallenge()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.AoChallenge;

            var posts = allData.Posts.Where(p => (p.Date.Month == 11 || (p.Date.Month == 12 && p.Date.Day <= 14) ) && p.Date.Year == DateTime.Now.Year).ToList();

            var firstDay = new DateTime(DateTime.Now.Year, 11, 1);
            var lastDay = new DateTime(DateTime.Now.Year, 12, 13);
            SetCurrentRows(posts, firstDay, lastDay, false);

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
            SetCurrentRows(posts, firstDay, lastDay, false);

            loading = false;
            await RefreshDropdowns();
        }

        private async Task ShowQSource()
        {
            loading = true;
            await Task.Delay(1);
            currentView = OverallView.QSource;

            var posts = allData.QSourcePosts;

            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDay = DateTime.Now;
            SetCurrentRows(posts, null, DateTime.Now, false);

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
            selectedPaxQSourcePosts = allData.QSourcePosts?.Where(p => p.Pax == row.PaxName).OrderByDescending(x => x.Date).ToList();
            selectedPaxHistoricalData = allData.HistoricalData?.FirstOrDefault(p => p.PaxName == row.PaxName);
            selectedPax = allData.Pax.FirstOrDefault(p => p.Name.Equals(row.PaxName, StringComparison.InvariantCultureIgnoreCase ));

            if (currentView == OverallView.AoChallenge || currentView == OverallView.AoList)
            {
                showAoChallengeModal = true;
            }
            else
            {
                await ShowModal();
            }
        }

        private async Task ExportToCsv()
        {
            var properties = typeof(DisplayRow).GetProperties();
            var csv = new StringBuilder();

            // Add header row
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Add data rows
            foreach (var item in currentRows)
            {
                csv.AppendLine(string.Join(",", properties.Select(p => p.GetValue(item, null))));
            }

            await JSRuntime.InvokeVoidAsync("downloadFile", $"F3_{RegionInfo.QueryStringValue}_{DateTime.Now.ToShortDateString()}_{currentView}.csv", csv.ToString());
        }
    }
}