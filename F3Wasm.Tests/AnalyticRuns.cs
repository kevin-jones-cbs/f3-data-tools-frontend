using System.Text;
using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class AnalyticRuns
    {
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