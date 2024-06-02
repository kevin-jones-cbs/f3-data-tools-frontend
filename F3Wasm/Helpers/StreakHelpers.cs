using F3Core.Regions;
using F3Wasm.Models;

namespace F3Wasm.Helpers
{
    public static class StreakHelpers
    {
        public static (int, DateTime) CalculateStreak(List<Post> posts, Region region)
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
                    (workoutDay.Date.DayOfWeek == DayOfWeek.Monday && (workoutDay.Date < firstSundayOpp || region.DisplayName != "South Fork") && workoutDay.Date == lastWorkoutDate.Value.AddDays(2))) // Handle before we had Sundays
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
    }
}
