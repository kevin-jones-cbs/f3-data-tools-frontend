using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using F3Lambda.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Momento.Sdk;
using Momento.Sdk.Auth;
using Momento.Sdk.Config;
using Momento.Sdk.Responses;
using F3Core;
using F3Core.Regions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace F3Lambda;

public class Function
{
    // Cache
    private ICredentialProvider authProvider = new EnvMomentoTokenProvider("F3_MOMENTO_AUTH_TOKEN");
    private TimeSpan DEFAULT_TTL = TimeSpan.FromHours(24);
    const string cacheName = "F3Data";
    private Region region;

    public async Task<object> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        try
        {
            // Deserialize the request body into a FunctionInput object
            var functionInput = System.Text.Json.JsonSerializer.Deserialize<FunctionInput>(request.Body);

            // For Cold Starts
            if (functionInput.Action == "Awake")
            {
                return "Awake";
            }

            // Get the region
            region = RegionList.GetRegion(functionInput.Region);

            if (region == null)
            {
                return "Error, no region specified";
            }

            // Set testing spreadsheet id if testing
            var spreadsheetId = region.RealSpreadsheetId;
            if (functionInput.IsTesting)
            {
                spreadsheetId = region.TestingSpreadsheetId;
            }

            // Get recent posts
            if (functionInput.Action == "GetMissingAos")
            {
                var sheetsService = GetSheetsService();
                var recentPosts = await GetMissingAosAsync(sheetsService, spreadsheetId);
                return recentPosts;
            }

            // Get The Pax
            if (functionInput.Action == "GetPax")
            {
                var sheetsService = GetSheetsService();
                var paxNames = await GetPaxNamesAsync(sheetsService, spreadsheetId);
                return paxNames;
            }

            // Add Pax
            if (functionInput.Action == "AddPax")
            {
                var sheetsService = GetSheetsService();
                await AddPaxToSheetAsync(sheetsService, spreadsheetId, functionInput.Pax, functionInput.QDate, functionInput.AoName, functionInput.IsTesting);
                return "Pax Added";
            }

            // Get all posts
            if (functionInput.Action == "GetAllPosts")
            {
                var sheetsService = GetSheetsService();
                var allPosts = await GetAllDataAsync(sheetsService, spreadsheetId, functionInput.IsTesting);
                return allPosts;
            }

            return "Error, unknown action";
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "Error" + ex.Message;
        }
    }

    private async Task<string> GetAllDataAsync(SheetsService sheetsService, string spreadsheetId, bool isTesting)
    {
        using (SimpleCacheClient client = new SimpleCacheClient(Configurations.Laptop.Latest(), authProvider, DEFAULT_TTL))
        {
            CacheGetResponse getResponse = await client.GetAsync(cacheName, region.GetCacheKey(isTesting));
            if (getResponse is CacheGetResponse.Hit hitResponse)
            {
                Console.WriteLine("Cache Hit");
                return hitResponse.ValueString;
            }

            var valueRange = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, "Master Data!B2:O").ExecuteAsync();
            var posts = valueRange.Values.Select(x => new Post
            {
                Date = DateTime.Parse(x[0].ToString()),
                Site = x.Count > 9 ? x[9].ToString() : string.Empty,
                Pax = x.Count > 10 ? x[10].ToString() : string.Empty,
                IsQ = x.Count > 13 ? x[13].ToString() == "1" : false,
            }).ToList();

            // Get the roster
            var result = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, "Roster!B4:C").ExecuteAsync();
            var pax = result.Values.Select(x => new Pax
            {
                Name = x[0].ToString(),
                DateJoined = x.Count == 2 && x[1].ToString().Contains("/") ? DateTime.Parse(x[1].ToString()).ToShortDateString() : string.Empty
            }).ToList();

            var rtn = new AllData
            {
                Posts = posts,
                Pax = pax
            };

            // Json serialize the object as small in size as possible
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(rtn, options);

            // Compress the json
            var compressedJson = Compress(json);

            // Save to cache
            var setResponse = await client.SetAsync(cacheName, region.GetCacheKey(isTesting), compressedJson);
            if (setResponse is CacheSetResponse.Error setError)
            {
                Console.WriteLine($"Error setting cache value: {setError.Message}.");
            }

            // Deserialize the json
            return compressedJson;
        }

        // Inline Functions
        static string Compress(string plainText)
        {
            var buffer = Encoding.UTF8.GetBytes(plainText);
            using var memoryStream = new MemoryStream();

            var lengthBytes = BitConverter.GetBytes((int)buffer.Length);
            memoryStream.Write(lengthBytes, 0, lengthBytes.Length);

            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            gZipStream.Write(buffer, 0, buffer.Length);
            gZipStream.Flush();

            var gZipBuffer = memoryStream.ToArray();

            return Convert.ToBase64String(gZipBuffer);
        }
    }

    private async Task<List<Ao>> GetMissingAosAsync(SheetsService sheetsService, string spreadsheetId)
    {
        try
        {
            var rtn = new List<Ao>();
            var valueRange = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, $"Master Data!{region.RangeForGettingRowCount}").ExecuteAsync();

            // Assign the first column to a DateTime and the last column to a string
            var qDates = valueRange.Values.Select(x => new { Dates = DateTime.Parse(x[0].ToString()), AoName = x[9].ToString() }).ToList();

            // Foreach loop for the last 7 days
            for (int i = 7; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                var dayOfWeek = date.DayOfWeek;

                // Get the AOs for the day of the week
                var aos = region.AoList.Where(x => x.DayOfWeek == dayOfWeek).ToList();
                foreach (var ao in aos)
                {
                    // Check if there is a post for the date and AO
                    var postExists = qDates.Any(x => x.Dates.Date == date.Date && x.AoName == ao.Name);
                    if (!postExists)
                    {
                        // Add the missing post to the list
                        var missingAo = new Ao
                        {
                            Name = ao.Name,
                            City = ao.City,
                            DayOfWeek = ao.DayOfWeek,
                            Date = date
                        };

                        rtn.Add(missingAo);
                    }
                }
            }

            return rtn;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private async Task<List<string>> GetPaxNamesAsync(SheetsService sheetsService, string spreadsheetId)
    {
        var result = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, "Roster!B:B").ExecuteAsync();
        var paxMembers = result.Values.Select(x => x.FirstOrDefault().ToString()).Distinct().ToList();

        return paxMembers;
    }

    private async Task AddPaxToSheetAsync(SheetsService sheetsService, string spreadsheetId, List<Pax> pax, DateTime qDate, string ao, bool isTesting)
    {
        // Get the sheet to find out the row count
        var masterDataCount = await GetSheetRowCountAsync(sheetsService, spreadsheetId, "Master Data!A:A");

        // Add the date once for each pax
        var updateDateCellsRequest = new UpdateCellsRequest
        {
            Start = new GridCoordinate
            {
                SheetId = region.MasterDataSheetId,
                RowIndex = masterDataCount,
                ColumnIndex = 1
            },
            Rows = new List<RowData>(),

            Fields = "userEnteredValue"
        };

        foreach (var member in pax)
        {
            updateDateCellsRequest.Rows.Add(new RowData
            {
                Values = new List<CellData> { new CellData { UserEnteredValue = new ExtendedValue { NumberValue = qDate.Date.ToOADate() } } }
            });
        }

        // Add each pax
        var updatePaxCellsRequest = new UpdateCellsRequest
        {
            Start = new GridCoordinate
            {
                SheetId = region.MasterDataSheetId,
                RowIndex = masterDataCount,
                ColumnIndex = 10
            },
            Rows = new List<RowData>(),
            Fields = "userEnteredValue"
        };

        foreach (var member in pax)
        {
            updatePaxCellsRequest.Rows.Add(new RowData
            {
                Values = new List<CellData>
                {
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = ao } },
                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = member.Name } },
                    new CellData { UserEnteredValue = new ExtendedValue { NumberValue = member.IsFng ? 1 : (double?)null } },
                    new CellData { UserEnteredValue = new ExtendedValue { NumberValue = 1 } },
                    new CellData { UserEnteredValue = new ExtendedValue { NumberValue = member.IsQ ? 1 : (double?)null } }
                }
            });
        }

        // Use the Google Sheets API to regex find the text Updated followed by the date and replace the date with the current date. Only search on the Master Data sheet
        var findReplaceRequest = new FindReplaceRequest
        {
            Find = "UPDATED \\d{1,2}\\/\\d{1,2}\\/\\d{4}",
            Replacement = $"UPDATED {DateTime.Now.ToShortDateString()}",
            MatchCase = false,
            MatchEntireCell = true,
            SearchByRegex = true,
            Range = new GridRange
            {
                SheetId = region.MasterDataSheetId,
                StartRowIndex = 0,
                EndRowIndex = int.MaxValue,
                StartColumnIndex = 11,
                EndColumnIndex = 12
            }
        };

        // Do a BatchUpdate with the FindReplaceRequest
        var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request>
            {
                new Request { FindReplace = findReplaceRequest },
                new Request { UpdateCells = updateDateCellsRequest },
                new Request { UpdateCells = updatePaxCellsRequest }
            }
        };

        // If there are any fngs, do another UpdateCellsRequest
        if (pax.Any(x => x.IsFng))
        {
            // Get the number of roster rows
            var rosterCount = await GetSheetRowCountAsync(sheetsService, spreadsheetId, "Roster!A:A");

            var updateFngCellsRequest = new UpdateCellsRequest
            {
                Start = new GridCoordinate
                {
                    SheetId = region.MasterDataSheetId,
                    RowIndex = rosterCount,
                    ColumnIndex = 1
                },
                Rows = new List<RowData>(),
                Fields = "userEnteredValue"
            };

            foreach (var member in pax.Where(x => x.IsFng))
            {
                updateFngCellsRequest.Rows.Add(new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = member.Name } },
                        new CellData { UserEnteredValue = new ExtendedValue { NumberValue = qDate.ToOADate() } }
                    }
                });
            }

            batchUpdateRequest.Requests.Add(new Request
            {
                UpdateCells = updateFngCellsRequest
            });
        }

        var batchUpdateResponse = await sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).ExecuteAsync();

        try
        {
            // Purge the cache
            using (SimpleCacheClient client = new SimpleCacheClient(Configurations.Laptop.Latest(), authProvider, DEFAULT_TTL))
            {
                await client.DeleteAsync(cacheName, region.GetCacheKey(isTesting));
            }
        }
        catch (Exception ex)
        {
            // Log it but don't throw
            Console.WriteLine("Error purging cache " + ex.Message);
        }
    }

    private async Task<int> GetSheetRowCountAsync(SheetsService sheetsService, string spreadsheetId, string range)
    {
        var valueRange = await sheetsService.Spreadsheets.Values.Get(spreadsheetId, range).ExecuteAsync();
        int numRows = valueRange.Values.Count;

        return numRows;
    }

    private static SheetsService GetSheetsService()
    {
        // If Debugger attached, we're local so svc account creds have extra prefix
        var prefix = string.Empty;

        using (var stream = new FileStream($"{prefix}Secrets/SvcAct.json", FileMode.Open, FileAccess.Read))
        {
            var scopes = new[] { SheetsService.Scope.Spreadsheets };

            var credentials = GoogleCredential.FromStream(stream)
                                   .CreateScoped(scopes)
                                   .UnderlyingCredential as ServiceAccountCredential;

            // Create the service.
            var sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                ApplicationName = "F3PaxToSheets",
                HttpClientInitializer = credentials
            });

            return sheetsService;
        }
    }
}
