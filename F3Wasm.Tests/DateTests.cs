using F3Core;
using F3Core.Regions;
using F3Wasm.Data;
using F3Wasm.Models;
using Xunit;

namespace F3Wasm.Tests
{
    public class DateTests
    {
        [Fact]
        public void NoSunday()
        {
            var posts = new List<Post>()
            {
                new Post() { Date = DateTime.Parse("1/27/2023"), Site = "The Public", Pax = "Manny Pedi" },
                new Post() { Date = DateTime.Parse("1/29/2023"), Site = "The Ditch", Pax = "Manny Pedi" },
                new Post() { Date = DateTime.Parse("1/29/2023"), Site = "Stargate", Pax = "Doodles" },
            };

            var result = F3Wasm.Pages.Data.GetCurrentPossibleWorkoutDays(posts);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void WithAFullMoon()
        {
            var posts = new List<Post>()
            {
                new Post() { Date = DateTime.Parse("1/27/2023"), Site = "The Public", Pax = "Manny Pedi" },
                new Post() { Date = DateTime.Parse("1/29/2023"), Site = "The Ditch", Pax = "Manny Pedi" },
                new Post() { Date = DateTime.Parse("1/29/2023"), Site = "Stargate", Pax = "Doodles" },
                new Post() { Date = DateTime.Parse("1/29/2023"), Site = "Full Moon Ruck", Pax = "Manny Pedi" },
            };

            var result = F3Wasm.Pages.Data.GetCurrentPossibleWorkoutDays(posts);

            Assert.Equal(3, result.Count);
        }
    }
}