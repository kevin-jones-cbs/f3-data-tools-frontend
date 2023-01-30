using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class DateTests
    {
        [Theory]
        [InlineData("1/29/2023", "1/27/2023", 2)]
        public void AllOfficialNames(string todaysDate, string firstPost, int expectedPercent)
        {
            var result = F3Wasm.Pages.Data.GetDaysSince(DateTime.Parse(todaysDate), DateTime.Parse(firstPost));
            Assert.Equal(expectedPercent, result);
        }
    }
}