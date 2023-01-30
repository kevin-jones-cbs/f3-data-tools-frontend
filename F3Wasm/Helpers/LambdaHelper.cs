using System.Text;
using System.Text.Json;
using F3Wasm.Models;

namespace F3Wasm.Data
{
    public static class LambdaHelper
    {
        // Get Missing Aos
        public static async Task<List<Ao>> GetMissingAosAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetMissingAos" });
            var missingAos = JsonSerializer.Deserialize<List<Ao>>(response);
            return missingAos;
        }

        public static async Task<List<string>> GetPaxNamesAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetPax"});
            var paxNames = JsonSerializer.Deserialize<List<string>>(response);
            return paxNames;
        }

        // Upload Pax 
        public static async Task UploadPaxAsync(HttpClient client, List<Pax> pax, string ao, DateTime qDate)
        {
            var input = new FunctionInput { Action = "AddPax", AoName = ao, QDate = qDate, Pax = pax };
            var json = JsonSerializer.Serialize(input);
            var response = await CallF3LambdaAsync(client, input);
            
        }

        // Get All Data
        public static async Task<AllData> GetAllDataAsync(HttpClient client)
        {
            var response = await CallF3LambdaAsync(client, new FunctionInput { Action = "GetAllPosts" });
            var allData = JsonSerializer.Deserialize<AllData>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            allData.Pax = allData.Pax.Where(p => p != null).ToList();
            allData.Posts = allData.Posts.Where(p => p != null).ToList();

            return allData;
        }

        private static async Task<string> CallF3LambdaAsync(HttpClient client, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://s6oww3m3a5svbuxq5pf35pjigu0xxaqk.lambda-url.us-west-1.on.aws/") { Content = content };
            request.Headers.Add("Access-Control-Allow-Origin", "*");
            request.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}