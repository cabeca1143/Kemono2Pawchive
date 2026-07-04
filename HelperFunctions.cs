using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Kemono2Pawchive;

internal static class HelperFunctions
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true
    };

    public static HttpClient SetupHttpClient(string baseAddress, out CookieContainer cookieContainer)
    {
        Uri kemonoAddress = new(baseAddress);
        cookieContainer = new();
        HttpClientHandler handler = new() { CookieContainer = cookieContainer };

        HttpClient client = new(handler) { BaseAddress = kemonoAddress };
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/css"));
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return client;
    }
}
