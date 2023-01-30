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
        public CurrentDisplay currentDisplay { get; set; }
        protected override async Task OnInitializedAsync()
        {
            allData = await LambdaHelper.GetAllDataAsync(Http);

            // Log how many pax and posts
            if (allData == null)
                Console.WriteLine("allData is null");
            else
                Console.WriteLine("allData is not null");

            // Group the posts by pax name
            var paxPosts = allData.Posts.GroupBy(p => p.P).ToDictionary(g => g.Key, g => g.Count());
            currentDisplay = new CurrentDisplay
            {
                Columns = new List<string> { "Name", "PostCount", "FirstPost", "Percent" },
                Rows = new List<DisplayRow>()
            };

            // Add the pax to the display
            foreach (var pax in paxPosts)
            {
                var row = new DisplayRow
                {
                    PaxName = pax.Key,
                    PostCount = pax.Value
                };

                DateTime.TryParse(allData.Pax.FirstOrDefault(p => p.Name == pax.Key)?.DateJoined, out DateTime date);
                if (date != DateTime.MinValue)
                {
                    row.FirstPost = date;
                    row.PostPercent = (((double)row.PostCount / (double)GetDaysSince(DateTime.Now, row.FirstPost.Value)) * 100);
                }             

                currentDisplay.Rows.Add(row);
            }

            currentDisplay.Rows = currentDisplay.Rows.OrderByDescending(r => r.PostPercent).ToList();
        }

        Task OnPaxSearchChanged(string value)
        {
            // Search currentDisplay.Rows where value1 contains value case insensitive
            currentDisplay.Rows = currentDisplay.Rows.Where(r => r.PaxName.ToString().ToLower().Contains(value.ToLower())).ToList();
            return Task.CompletedTask;            
        }

        public static int GetDaysSince(DateTime today, DateTime date)
        {
            if (date == null)
                return -1;

            // Return the number of days since the date
            var days = (today - date).Days + 1;            
            
            // Return the number of sundays since the date
            var sundays = Enumerable.Range(0, days + 1).Count(d => date.AddDays(d).DayOfWeek == DayOfWeek.Sunday);            

            return days - sundays;
        }
    }
}