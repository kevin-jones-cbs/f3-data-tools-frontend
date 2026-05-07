using F3Core;
using F3Core.Regions;
using Xunit;

namespace F3Wasm.Tests
{
    public class StreakTests
    {
        [Fact]
        public void CalculateStreak_UsesPossibleWorkoutDays()
        {
            var workoutDates = new[]
            {
                new DateTime(2026, 4, 20),
                new DateTime(2026, 4, 22),
                new DateTime(2026, 4, 24),
                new DateTime(2026, 4, 25),
                new DateTime(2026, 4, 27),
                new DateTime(2026, 4, 29),
                new DateTime(2026, 5, 1),
                new DateTime(2026, 5, 2),
            };

            var posts = workoutDates
                .Select(date => new Post { Date = date, Site = "Plumas Lake", Pax = "Spirit Fingers" })
                .ToList();

            var allPossibleWorkoutDays = workoutDates
                .Select(date => new WorkoutDay { Date = date })
                .ToList();

            var (streak, streakStart) = StreakHelpers.CalculateStreak(posts, new PlumasLake(), allPossibleWorkoutDays);

            Assert.Equal(8, streak);
            Assert.Equal(new DateTime(2026, 4, 20), streakStart);
        }
    }
}
