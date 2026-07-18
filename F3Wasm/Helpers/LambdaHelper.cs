using System.Buffers;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using F3Wasm.Models;
using F3Core;
using F3Core.Regions;

namespace F3Wasm.Data
{
    public static class LambdaHelper
    {
        // Get Missing Aos
        public static async Task<List<Ao>> GetMissingAosAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetMissingAos, Region = region });
            var missingAos = JsonSerializer.Deserialize<List<Ao>>(response);
            return missingAos;
        }

        public static async Task<List<string>> GetPaxNamesAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetPax, Region = region });
            var paxNames = JsonSerializer.Deserialize<List<string>>(response);
            return paxNames;
        }

        // Get Pax from Comment
        public static async Task<List<Pax>> GetPaxFromCommentAsync(HttpClient client, string region, string comment)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetPaxFromComment, Region = region, Comment = comment });
            var pax = JsonSerializer.Deserialize<List<Pax>>(response);
            return pax;
        }

        // Upload Pax 
        public static async Task UploadPaxAsync(HttpClient client, string region, List<Pax> pax, string ao, DateTime qDate, bool isQSource)
        {
            var input = new FunctionInput { Action = LambdaActions.AddPax, AoName = ao, QDate = qDate, Pax = pax, Region = region, IsQSource = isQSource };
            var json = JsonSerializer.Serialize(input);
            var response = await CallF3LambdaAsync(client, input);
        }

        public static async Task<AllData> GetAllDataAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetAllPosts, Region = region });
            return DecompressAll(response);
        }

        public static async Task<InitialViewData> GetInitialViewAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetInitialView, Region = region });
            var initialViewData = JsonSerializer.Deserialize<InitialViewData>(response);
            return initialViewData;
        }

        public static async Task<List<RegionMetadata>> GetRegionsAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetRegions });
            return JsonSerializer.Deserialize<List<RegionMetadata>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RegionMetadata>();
        }

        public static async Task<Region> GetRegionAsync(HttpClient client, string region)
        {
            var hardCodedRegion = RegionList.All.FirstOrDefault(x => x.QueryStringValue == region);
            if (hardCodedRegion != null)
            {
                return hardCodedRegion;
            }

            var metadata = (await GetRegionsAsync(client))
                .FirstOrDefault(x => string.Equals(x.QueryStringValue, region, StringComparison.OrdinalIgnoreCase));

            if (metadata == null)
            {
                return null;
            }

            return new ConfiguredRegion(new RegionConfig
            {
                QueryStringValue = metadata.QueryStringValue,
                DisplayName = metadata.DisplayName,
                SupportsDownrange = metadata.SupportsDownrange,
                HasQSourcePosts = metadata.HasQSourcePosts,
                HasExtraActivity = metadata.HasExtraActivity,
                IncludeInSector = metadata.IncludeInSector,
                IsActive = true
            });
        }

        public static async Task<List<Ao>> GetAllLocationsAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetLocations, Region = region });
            var locations = JsonSerializer.Deserialize<List<Ao>>(response);
            return locations;
        }

        public static async Task<string> GetJsonAsync(HttpClient client, string region, short jsonRow)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetJson, Region = region, JsonRow = jsonRow });
            return response;
        }

        public static async Task SaveJsonAsync(HttpClient client, string region, short jsonRow, string json)
        {
            var input = new FunctionInput { Action = LambdaActions.SaveJson, Region = region, JsonRow = jsonRow, Json = json };
            var response = await CallF3LambdaAsync(client, input);
        }

        public static async Task ClearCacheAsync(HttpClient client, string region)
        {
            await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.ClearCache, Region = region });
        }

        public static async Task<SectorData> GetSectorDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetSectorDataSummaryAsync});

            // Decompress the response (same as GetAllDataAsync)
            var decompressed = Decompress(response);
            var sectorData = JsonSerializer.Deserialize<SectorData>(decompressed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return sectorData;
        }

        public static async Task<List<TerracottaChallenge>> GetTerracottaChallengeDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetTerracottaChallenge, Region = "terracotta" });
            var terracottaChallengeData = JsonSerializer.Deserialize<List<TerracottaChallenge>>(response);
            return terracottaChallengeData;
        }

        public static async Task<List<ForgeChallenge>> GetForgeChallengeDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetForgeChallenge, Region = "motherlode" });
            var forgeChallengeData = JsonSerializer.Deserialize<List<ForgeChallenge>>(response);
            return forgeChallengeData;
        }

        public static async Task<TowerChallengeResponse> GetTowerChallengeDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = LambdaActions.GetTowerChallenge, Region = "sactown" });
            var towerChallengeData = JsonSerializer.Deserialize<TowerChallengeResponse>(response);
            return towerChallengeData ?? new TowerChallengeResponse();
        }

        public static AllData DecompressAll(string compressedString)
        {
            var decompressed = Decompress(compressedString);

            var allData = JsonSerializer.Deserialize<AllData>(decompressed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            allData.Pax = allData.Pax.Where(p => p != null).ToList();
            allData.Posts = allData.Posts.Where(p => p != null).ToList();

            return allData;
        }

        private static string Decompress(string compressedText)
        {
            var gZipBuffer = Convert.FromBase64String(compressedText);

            using var memoryStream = new MemoryStream();
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];
            memoryStream.Position = 0;

            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);

            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = gZipStream.Read(buffer, totalRead, buffer.Length - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            return Encoding.UTF8.GetString(buffer);
        }

        private static async Task<string> CallF3LambdaAsync(HttpClient client, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, string.Empty) { Content = content };
            request.Headers.Add("Access-Control-Allow-Origin", "*");
            request.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public static async Task<List<ExiconEntry>> SearchExiconEntriesAsync(HttpClient client, string term)
        {
            // Anonymouse object with Term property
            var input = JsonSerializer.Serialize(new { term });
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var lambdaUrl = "https://wdd5t63r4ve5btbdv5yypcnugq0fgeow.lambda-url.us-west-1.on.aws/";

            var request = new HttpRequestMessage(HttpMethod.Post, lambdaUrl) { Content = content };
            request.Headers.Add("Access-Control-Allow-Origin", "*");
            request.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            // This returns {id: 1, score: 0.782}
            var entries = JsonSerializer.Deserialize<List<ExiconEntry>>(responseString);

            // Fill in the rest of the data
            var allEntries = ExiconData.Entries;
            var one = allEntries.FirstOrDefault();
            foreach (var entry in entries)
            {
                var fullEntry = allEntries.FirstOrDefault(x => x.Id == entry.Id);
                entry.Term = fullEntry.Term;
                entry.Description = fullEntry.Description;
            }
            
            return entries;
        }
    }
}
