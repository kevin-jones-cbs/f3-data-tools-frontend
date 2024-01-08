using ApexCharts;
using F3Wasm.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic;

namespace F3Wasm.Components
{
    public class KeyDateDisplay
    {
        public DateTime Date { get; set; }
        public int PostCount { get; set; }
    }

    public partial class PaxKeyDates
    {
        [Parameter]
        public List<Post> PaxPosts { get; set; } = new();

        private ApexChartOptions<KeyDateDisplay> options = new();

        public List<KeyDateDisplay> KeyPostDates { get; set; } = new();
        public List<KeyDateDisplay> KeyQDates { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            // Chart Options
            options.Xaxis = new XAxis
            {
                Type = XAxisType.Datetime
            };

            options.Tooltip = new Tooltip
            {
                X = new TooltipX
                {
                    Format = "MMM dd yyyy"
                }
            };

            // Chart Series
            var posts = PaxPosts.OrderBy(x => x.Date).ToList();
            var qs = posts.Where(x => x.IsQ).ToList();

            KeyPostDates.Add(new KeyDateDisplay { Date = posts.FirstOrDefault().Date, PostCount = 1 });

            if (qs.Count > 0)
            {
                KeyQDates.Add(new KeyDateDisplay { Date = qs.FirstOrDefault().Date, PostCount = 1 });
            }

            // Add each 100th post
            for (var i = 100; i < posts.Count; i += 100)
            {
                KeyPostDates.Add(new KeyDateDisplay { Date = posts[i - 1].Date, PostCount = i });
            }

            // Add the last post
            KeyPostDates.Add(new KeyDateDisplay { Date = posts.LastOrDefault().Date, PostCount = posts.Count });

            // Add each 10th Q
            for (var i = 10; i < qs.Count; i += 10)
            {
                KeyQDates.Add(new KeyDateDisplay { Date = qs[i - 1].Date, PostCount = i });
            }

            await Task.Delay(10);
        }
    }
}