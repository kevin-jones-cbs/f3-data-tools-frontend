using System.Text;
using F3Core.Regions;
using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class AnalyticRuns
    {
        [Fact]
        public async Task GetLocationCountsForMonth()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var thisMonthPosts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.Year && p.Date.Month == 4).ToList();

            // Group this month posts by ao, order by count descending
            var postsByAo = thisMonthPosts.GroupBy(p => p.Site).OrderByDescending(g => g.Count()).ToList();

            // Print to console
            foreach (var ao in postsByAo)
            {
                Console.WriteLine($"{ao.Key} - {ao.Count()}");
            }
        }

        [Fact]
        public async Task GetAllFullMoonRucks()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var rucks = allData.Posts.Where(x => x.Site == "Full Moon Ruck").ToList();

            // Group by pax
            var pax = rucks.GroupBy(r => r.Pax).OrderByDescending(g => g.Count()).ToList();

            // Print to console
            foreach (var p in pax)
            {
                Console.WriteLine($"{p.Key} - {p.Count()}");
            }
        }

        [Fact]
        public async Task GetQSummary()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var thisMonthPosts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.Year && p.Date.Month == 4).ToList();

            // Group by Pax Name that have isQ
            var qPax = thisMonthPosts.Where(p => p.IsQ).GroupBy(p => p.Pax).OrderByDescending(g => g.Count()).ToList();

            // Print to console
            foreach (var pax in qPax)
            {
                Console.WriteLine($"{pax.Key} - {pax.Count()}");
            }
        }

        [Fact]
        public async Task GetStaleRoster()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");

            // Get a list of Pax with their post count, and the date of their last post
            var pax = allData.Pax.Select(p => new
            {
                p.Name,
                PostCount = allData.Posts.Count(po => po.Pax.Equals(p.Name)),
                LastPost = allData.Posts.Where(po => po.Pax.Equals(p.Name)).OrderByDescending(po => po.Date).FirstOrDefault()?.Date
            }).ToList();

            // Only show Pax that have not posted in the last year
            var stalePax = pax.Where(p => p.LastPost != null && (DateTime.Now - p.LastPost.Value).Days > 365).OrderByDescending(p => p.LastPost).ToList();

            // Exclude anyone with more than 10 posts
            stalePax = stalePax.Where(p => p.PostCount < 10).ToList();

            // Save to a csv file
            var csv = new StringBuilder();
            csv.AppendLine("Name,Post Count,Last Post");
            foreach (var p in stalePax)
            {
                csv.AppendLine($"{p.Name},{p.PostCount},{p.LastPost?.ToShortDateString()}");
            }

            File.WriteAllText("stale_pax.csv", csv.ToString());
        }

        [Fact]
        public async Task GetAveragePostsPerDay()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var thisYearPosts = allData.Posts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();

            // Group the posts by date
            var postsByDate = thisYearPosts.GroupBy(p => p.Date.Date).ToList();

            // Get the average posts per day
            var averagePostsPerDay = postsByDate.Average(g => g.Count());

            // Group this year posts by ao, order by count descending
            var postsByAo = thisYearPosts.GroupBy(p => p.Site).OrderByDescending(g => g.Count()).ToList();

            // Print to console
            foreach (var ao in postsByAo)
            {
                Console.WriteLine($"{ao.Key} - {ao.Count()}");
            }

            // Get the posts at the grid for this year
            var gridPosts = thisYearPosts.Where(p => p.Site == "The Grid").ToList();
            var gridPostsNoKids = thisYearPosts.Where(p => p.Site == "The Grid" && !p.Pax.Contains("2.")).ToList();

            // Get the number of saturdays that there have been this year, in general
            var saturdays = thisYearPosts.Where(p => p.Date.DayOfWeek == DayOfWeek.Saturday).Select(p => p.Date.Date).Distinct().ToList();

            // Get the average posts per day on saturdays at the grid
            var averagePostsPerDayOnSaturdays = gridPostsNoKids.Where(p => p.Date.DayOfWeek == DayOfWeek.Saturday).GroupBy(p => p.Date.Date).Average(g => g.Count());


        }

        [Fact]
        public async Task GetKotterList()
        {
            // Editable Params
            var minPostCount = 1;
            var minDaysSinceLastPost = 25;
            var maxDaysSinceLastPost = 200;

            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var asgardData = await LambdaHelper.GetAllDataAsync(client, "asgard");
            var posts = allData.Posts;
            var pax = allData.Pax;

            var kotterList = new List<Kotter>();
            var asgardKotter = new List<Kotter>();

            foreach (var member in pax)
            {
                var lastPost = posts.Where(p => p.Pax.Equals(member.Name)).OrderByDescending(p => p.Date).FirstOrDefault();

                if (lastPost == null)
                {
                    continue;
                }

                var postCount = posts.Count(p => p.Pax.Equals(member.Name));
                var daysSinceLastPost = (DateTime.Now - lastPost.Date).Days;

                if (postCount > minPostCount && daysSinceLastPost > minDaysSinceLastPost && daysSinceLastPost < maxDaysSinceLastPost)
                {
                    // Technically Kotter, check Asgard
                    var asgardLastPost = asgardData.Posts.Where(p => p.Pax.Equals(member.Name)).OrderByDescending(p => p.Date).FirstOrDefault();
                    if (asgardLastPost != null)
                    {
                        var daysSinceLastAsgardPost = (DateTime.Now - asgardLastPost.Date).Days;
                        if (daysSinceLastAsgardPost < daysSinceLastPost)
                        {
                            // Last post was at Asgard, different list
                            asgardKotter.Add(new Kotter
                            {
                                Name = member.Name,
                                DaysSinceLastPost = daysSinceLastPost,
                                PostCount = postCount,
                                LastPost = lastPost.Date,
                                LastAsgardPost = asgardLastPost.Date
                            });

                            continue;
                        }
                    }

                    kotterList.Add(new Kotter
                    {
                        Name = member.Name,
                        DaysSinceLastPost = daysSinceLastPost,
                        PostCount = postCount,
                        LastPost = lastPost.Date
                    });
                }
            }

            // Order by days since last post
            kotterList = kotterList.OrderBy(k => k.DaysSinceLastPost).ToList();

            // Write the results to a csv file
            var csv = new StringBuilder();
            csv.AppendLine("Name,Days Since Last Post,Post Count,Last Post");
            foreach (var kotter in kotterList)
            {
                csv.AppendLine($"{kotter.Name},{kotter.DaysSinceLastPost},{kotter.PostCount},{kotter.LastPost.ToShortDateString()}");
            }

            File.WriteAllText("kotter.csv", csv.ToString());

            // Asgard Kotter
            csv = new StringBuilder();
            csv.AppendLine("Name,Days Since Last Post,Post Count,Last Post,Last Asgard Post");
            foreach (var kotter in asgardKotter)
            {
                csv.AppendLine($"{kotter.Name},{kotter.DaysSinceLastPost},{kotter.PostCount},{kotter.LastPost.ToShortDateString()},{kotter.LastAsgardPost.ToShortDateString()}");
            }

            File.WriteAllText("asgard_kotter.csv", csv.ToString());
        }

        [Fact]
        public async Task GetTopPostCountsPerLocation()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var pax = allData.Pax;
            var posts = allData.Posts;

            var aoPosts = posts.Where(p => p.Site == "The Cut").ToList();

            // Only show this year
            aoPosts = aoPosts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();

            // Group each one by day to find the most posts in a day
            var postsByDay = aoPosts.GroupBy(p => p.Date.Date).OrderByDescending(g => g.Count()).ToList();

            // Write to File
            var csv = new StringBuilder();
            csv.AppendLine("Date,Post Count");
            foreach (var p in postsByDay)
            {
                csv.AppendLine($"{p.Key} - {p.Count()}");
            }

            File.WriteAllText("the_pit.csv", csv.ToString());
        }

        [Fact]
        public async Task GetTopPostersPerLocation()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var pax = allData.Pax;
            var posts = allData.Posts;

            var aoPosts = posts.Where(p => p.Site == "The Cut").ToList();

            // Only show this year
            aoPosts = aoPosts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();

            // Group each one by pax 
            var postsByDay = aoPosts.GroupBy(p => p.Pax).OrderByDescending(g => g.Count()).ToList();

            // Write to File
            var csv = new StringBuilder();
            csv.AppendLine("Pax,Post Count");
            foreach (var p in postsByDay)
            {
                csv.AppendLine($"{p.Key} - {p.Count()}");
            }

            File.WriteAllText("top_posters.csv", csv.ToString());
        }

        [Fact]
        public async Task GetTopPostersForAllLocations()
        {
            var client = new HttpClient();
            var allData = await LambdaHelper.GetAllDataAsync(client, "southfork");
            var pax = allData.Pax;
            var posts = allData.Posts;

            // Only show this year
            posts = posts.Where(p => p.Date.Year == DateTime.Now.Year).ToList();

            var region = new SouthFork();

            await File.WriteAllTextAsync("top_posters_all.csv", "");
            foreach (var location in region.AoList)
            {
                var regionPosts = posts.Where(p => p.Site == location.Name).ToList();
                // Group each one by pax 
                var postsByDay = regionPosts.GroupBy(p => p.Pax).OrderByDescending(g => g.Count()).Take(10).ToList();

                // Write to File
                var csv = new StringBuilder();
                csv.AppendLine($"\n\n{location.Name}:");
                foreach (var p in postsByDay)
                {
                    csv.AppendLine($"{p.Key} - {p.Count()}");
                }

                await File.AppendAllTextAsync("top_posters_all.csv", csv.ToString());
            }


        }
    }

    public class Kotter
    {
        public string Name { get; set; }
        public int DaysSinceLastPost { get; set; }
        public int PostCount { get; set; }
        public DateTime LastPost { get; set; }
        public DateTime LastAsgardPost { get; set; }
    }
}