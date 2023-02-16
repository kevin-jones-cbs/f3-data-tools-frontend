using F3Wasm.Data;
using Xunit;

namespace F3Wasm.Tests
{
    public class PostWithTests
    {
        [Theory]
        [InlineData("Pixar")]
        public void WithAQ(string paxName)
        {
            var compressed = TestData.CompressedAllData;
            var allData = LambdaHelper.DecompressAll(compressed);
            var selectedPaxPosts = allData.Posts.Where(p => p.Pax == paxName).ToList();
            var selectedPax100Count = selectedPaxPosts.Count / 100;

            var postWith = F3Wasm.Pages.Data.GetAllTimePaxPostWith("Recent100", selectedPaxPosts, allData, selectedPax100Count);
        }
    }
}