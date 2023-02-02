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
        public List<DisplayRow> currentRows { get; set; }
        public DateTime lastUpdatedDate { get; set; }

        protected override async Task OnInitializedAsync()
        {
            allData = await LambdaHelper.GetAllDataAsync(Http);
            var lastUpdateItem = allData.Posts.FirstOrDefault(x => x.Site == "UPDATE");
            if (lastUpdateItem != null)
            {
                lastUpdatedDate = DateTime.Parse(lastUpdateItem.Pax.Split(" ")[1]);
                allData.Posts.Remove(lastUpdateItem);
            }

            SetCurrentRows(allData.Posts);
        }

        public void SetCurrentRows(List<Post> posts)
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

                // DateTime.TryParse(allData.Pax.FirstOrDefault(p => p.Name == pax.Key)?.DateJoined, out DateTime date);
                var firstDate = posts.Where(p => p.Pax == pax.Key).Min(p => p.Date);
                if (firstDate != DateTime.MinValue)
                {
                    row.FirstPost = firstDate;
                    row.PostPercent = ((double)row.PostCount / (double)GetDaysSince(DateTime.Now, row.FirstPost.Value) * 100);
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
            SetCurrentRows(allData.Posts);
        }

        private void ShowPreviousMonth()
        {
            var posts = allData.Posts.Where(p => p.Date.Month == DateTime.Now.AddMonths(-1).Month && p.Date.Year == DateTime.Now.AddMonths(-1).Year).ToList();
            SetCurrentRows(posts);
        }

        private void ShowCurrentMonth()
        {
            var posts = allData.Posts.Where(p => p.Date.Month == DateTime.Now.Month && p.Date.Year == DateTime.Now.Year).ToList();
            SetCurrentRows(posts);
        }

        private void ShowPreviousYear()
        {
            var posts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.AddYears(-1).Year).ToList();
            SetCurrentRows(posts);
        }

        private void ShowCurrentYear()
        {
            var posts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();
            SetCurrentRows(posts);
        }
    }
}