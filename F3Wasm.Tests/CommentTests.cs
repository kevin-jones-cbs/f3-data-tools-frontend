using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class CommentTests
    {
        [Theory]
        [InlineData("@Peacock @Clark @Mani Pedi @Rollback @Chalupa @SofaKing @TraLaLa @One Tree Hill @PiXAR @Jigglypuff @Gumby @Cage Free @Deuce @Top40 @Mr. Meaner @Dead End @chippendale", 17)]
        public void JustNames(string comment, int expectedCount)
        {
            var result = PaxHelper.GetPaxFromComment(comment, TestData.PaxNames);

            Assert.Equal(result.Count, expectedCount);
        }

        [Theory]
        [InlineData("@Peacock", "Peacock")]
        [InlineData("@S’mores", "S'mores")]
        [InlineData("@Mr. No Name", "Mr. No Name (2.0)")]
        [InlineData("@Mani Pedi", "Manny Pedi")]
        [InlineData("@ROXBURY mike c.", "Roxbury")]
        public void SpecialNames(string comment, string expectedName)
        {
            var result = PaxHelper.GetPaxFromComment(comment, TestData.PaxNames);

            Assert.Equal(result.Count, 1);
            Assert.Equal(result[0].Name, expectedName);
        }

        [Theory]
        [InlineData("Q: @Peacock @Clark", "Peacock")]
        [InlineData("Q: Peacock @Clark @Peacock", "Peacock")]
        [InlineData("Q: @Spread'Em @Peacock", "Spread'em")]
        public void WithAQ(string comment, string expectedQ)
        {
            var result = PaxHelper.GetPaxFromComment(comment, TestData.PaxNames);

            Assert.Equal(result.Count, 2);
            Assert.Equal(result.Where(x => x.IsQ).Count(), 1);
            Assert.Equal(result.FirstOrDefault(x => x.IsQ).Name, expectedQ);
        }
    }
}