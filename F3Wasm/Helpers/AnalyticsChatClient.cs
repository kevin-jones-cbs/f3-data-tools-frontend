using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using F3Wasm.Models;

namespace F3Wasm.Helpers;

public sealed class AnalyticsChatClient(HttpClient http)
{
    public async Task<AnalyticsChatStatus> GetStatusAsync(CancellationToken ct, string region = "southfork")
    {
        using var response = await http.GetAsync("chat/status?region=" + Uri.EscapeDataString(region), ct);
        await CheckAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<AnalyticsChatStatus>(cancellationToken: ct))!;
    }

    public async Task<AnalyticsChatReply> SendAsync(List<AnalyticsChatMessage> messages, CancellationToken ct, string? conversationId = null, string? visitorId = null, string region = "southfork")
    {
        using var response = await http.PostAsJsonAsync("chat", new { messages, conversationId, visitorId, region }, ct);
        await CheckAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<AnalyticsChatReply>(cancellationToken: ct))!;
    }

    public async Task<AnalyticsChatReply> StreamAsync(List<AnalyticsChatMessage> messages,
        Func<string, string, Task> progress, CancellationToken ct, string conversationId, string visitorId, string region = "southfork")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/stream")
        { Content = JsonContent.Create(new { messages, conversationId, visitorId, region }) };
        request.SetBrowserResponseStreamingEnabled(true);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        await CheckAsync(response, ct);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(ct) is { } line)
        {
            using var json = JsonDocument.Parse(line);
            var root = json.RootElement;
            var type = root.GetProperty("type").GetString()!;
            if (type == "complete") return root.GetProperty("reply").Deserialize<AnalyticsChatReply>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            var text = root.GetProperty("text").GetString() ?? "";
            if (type == "error") throw new HttpRequestException(text);
            await progress(type, text);
        }
        throw new HttpRequestException("The connection ended before the answer finished. Please try again.");
    }

    public async Task<JsonElement> GetAdminAsync(string path, CancellationToken ct, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "admin/chats" + path);
        request.Headers.Add("X-Chat-Admin-Password", password);
        using var response = await http.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new HttpRequestException("Incorrect admin password.", null, response.StatusCode);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(response.StatusCode == System.Net.HttpStatusCode.Forbidden
                ? "Chat administration is available only on the local backend."
                : "Could not load saved chats. Check the backend and try again.");
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
    }

    private static async Task CheckAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        // Keep provider/configuration details out of the member-facing conversation.
        var message = (int)response.StatusCode switch
        {
            429 => "The assistant is busy. Please try again in a moment.",
            503 => "The assistant isn't set up yet. Please contact the app administrator.",
            504 => "That took a little too long. Try asking about a shorter date range.",
            _ => "I couldn't finish that question. Please try again or make it more specific."
        };
        await Task.CompletedTask;
        throw new HttpRequestException(message);
    }
}
