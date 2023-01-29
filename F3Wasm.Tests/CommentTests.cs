using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class CommentTests
    {
        [Theory]
        [InlineData("@Peacock @Clark @Mani Pedi @Rollback @S’mores @SofaKing @TraLaLa @One Tree Hill @PiXAR @Jigglypuff @Gumby @Cage Free @Deuce @Top40 @Mr. Meaner @Dead End @chippendale", 17)]
        [InlineData("Peacock Clark Mani Pedi Rollback S’mores SofaKing TraLaLa One Tree Hill PiXAR Jigglypuff Gumby Cage Free Deuce Top40 Mr. Meaner Dead End chippendale", 17)]
        [InlineData("Denali 1.25.23 PAX: 15 @Herbie @Happy Tree @Doodles @Potts @Singe @Deuce @Putt Putt @Shipwreck @Clark @SofaKing @Deep Dish @Baskinz @Daffodil @Switch @ROXBURY mike c.", 15)]
        [InlineData("Q: @Peacock PAX: @Spread’em @Roblox @Shipwreck @Happy Tree @gumby @Herbie @Daffodil @Richard Simmons @SweatShop - Hernan C @S’mores", 11)]
        [InlineData("VQ Dead End Switch Happy Tree Daffodil Mr. Meaner Rollback Baskinz Jigglypuff Chalupa The View Singe One Tree Hill Shipwreck TraLaLa XPort Doodles Cage Free PiXAR Brady Bunch Gumby SofaKing Deep Dish Turf Toe SweatShop - Hernan C Putt Putt Peacock ROXBURY mike c. chippendale", 28)]
        [InlineData(@"Denali 1.25.23
                    PAX: 15
                    @Herbie @Happy Tree @Doodles @Potts @Singe @Deuce @Putt Putt @Shipwreck @Clark @SofaKing @Deep Dish @Baskinz @Daffodil @Switch @ROXBURY mike c.", 15)]
        public void AllOfficialNames(string comment, int expectedCount)
        {
            var result = PaxHelper.GetPaxFromComment(comment, TestData.PaxNames);

            Assert.Equal(expectedCount, result.Where(x => x != null && x.IsOfficial).ToList().Count);
            Assert.Equal(0, result.Where(x => x != null && !x.IsOfficial).ToList().Count);
        }

        [Theory]
        [InlineData("@Peacock @Clark @Mani Pedi @Newonewordguy", 3, 1, "Newonewordguy")]
        [InlineData("Peacock Clark Mani Pedi Newonewordguy", 3, 1, "Newonewordguy")]
        [InlineData("Peacock Clark Mani Pedi Super Man", 3, 2, "Super")]
        public void WithUnofficialNames(string comment, int expectedOfficialCount, int expectedUnofficialCount, string expectedUnofficialName)
        {
            var result = PaxHelper.GetPaxFromComment(comment, TestData.PaxNames);

            Assert.Equal(expectedOfficialCount, result.Where(x => x != null && x.IsOfficial).ToList().Count);
            Assert.Equal(expectedUnofficialCount, result.Where(x => x != null && !x.IsOfficial).ToList().Count);

            Assert.Equal(expectedUnofficialName, result.Where(x => x != null && !x.IsOfficial).ToList()[0].UnknownName);
        }

        [Theory]
        [InlineData("@Peacock", "Peacock")]
        [InlineData("@S’mores", "S'mores")]
        [InlineData("@Mani Pedi", "Manny Pedi")]
        [InlineData("@ROXBURY mike c.", "Roxbury")]
        [InlineData("@Top40", "Top 40")]
        [InlineData("@Hill Billy", "Hillbilly")]
        [InlineData("Heat Check", "HeatCheck")]
        [InlineData("Linguine", "Linguini")]
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