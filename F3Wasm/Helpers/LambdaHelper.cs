using System.Buffers;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using F3Wasm.Models;
using F3Core;

namespace F3Wasm.Data
{
    public static class LambdaHelper
    {
        // Get Missing Aos
        public static async Task<List<Ao>> GetMissingAosAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetMissingAos", Region = region });
            var missingAos = JsonSerializer.Deserialize<List<Ao>>(response);
            return missingAos;
        }

        public static async Task<List<string>> GetPaxNamesAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetPax", Region = region });
            var paxNames = JsonSerializer.Deserialize<List<string>>(response);
            return paxNames;
        }

        // Get Pax from Comment
        public static async Task<List<Pax>> GetPaxFromCommentAsync(HttpClient client, string region, string comment)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetPaxFromComment", Region = region, Comment = comment });
            var pax = JsonSerializer.Deserialize<List<Pax>>(response);
            return pax;
        }

        // Upload Pax 
        public static async Task UploadPaxAsync(HttpClient client, string region, List<Pax> pax, string ao, DateTime qDate, bool isQSource)
        {
            var input = new FunctionInput { Action = "AddPax", AoName = ao, QDate = qDate, Pax = pax, Region = region, IsQSource = isQSource };
            var json = JsonSerializer.Serialize(input);
            var response = await CallF3LambdaAsync(client, input);
        }

        public static async Task<AllData> GetAllDataAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetAllPosts", Region = region });
            return DecompressAll(response);
        }

        public static async Task<List<Ao>> GetAllLocationsAsync(HttpClient client, string region)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetLocations", Region = region });
            var locations = JsonSerializer.Deserialize<List<Ao>>(response);
            return locations;
        }

        public static async Task<string> GetJsonAsync(HttpClient client, string region, short jsonRow)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetJson", Region = region, JsonRow = jsonRow });
            return response;
        }

        public static async Task SaveJsonAsync(HttpClient client, string region, short jsonRow, string json)
        {
            var input = new FunctionInput { Action = "SaveJson", Region = region, JsonRow = jsonRow, Json = json };
            var response = await CallF3LambdaAsync(client, input);
        }

        public static async Task ClearCacheAsync(HttpClient client, string region)
        {
            await CallF3LambdaAsync(client, new FunctionInput { Action = "ClearCache", Region = region });
        }

        public static async Task<SectorData> GetSectorDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetSectorData"});
            var sectorData = JsonSerializer.Deserialize<SectorData>(response);
            return sectorData;
        }

        public static AllData DecompressAll(string compressedString)
        {
            var decompressed = Decompress(compressedString);

            var allData = JsonSerializer.Deserialize<AllData>(decompressed, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            allData.Pax = allData.Pax.Where(p => p != null).ToList();
            allData.Posts = allData.Posts.Where(p => p != null).ToList();

            return allData;

            string Decompress(string compressedText)
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
        }

        private static async Task<string> CallF3LambdaAsync(HttpClient client, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var lambdaUrl = "https://s6oww3m3a5svbuxq5pf35pjigu0xxaqk.lambda-url.us-west-1.on.aws/";

            var request = new HttpRequestMessage(HttpMethod.Post, lambdaUrl) { Content = content };
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