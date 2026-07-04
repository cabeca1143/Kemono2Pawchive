using System.Net;
using System.Text;
using System.Text.Json;
using static Kemono2Pawchive.Credentials;

namespace Kemono2Pawchive.Kemono;

internal class KemonoProvider : IReadProvider<BaseFavArtist>
{
    private readonly string _baseAddress;

    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer;

    public bool IsLoggedIn { get; private set; }

    public KemonoProvider(string baseAddress)
    {
        _baseAddress = baseAddress;
        _httpClient = HelperFunctions.SetupHttpClient(_baseAddress, out _cookieContainer);
    }

    public async Task<bool> Login(Login login)
    {
        if (!string.IsNullOrEmpty(login.Cookie))
        {
            Console.WriteLine("Session cookie found. Testing cookie...");
            _cookieContainer.Add(new Uri(_baseAddress), new Cookie("session", login.Cookie));

            HttpResponseMessage testResponse = await _httpClient.GetAsync("/api/v1/account/favorites");
            if (testResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Logged in to Kemono successfully via Cookie!");
                return IsLoggedIn = true;
            }

            Console.WriteLine("Cookie was invalid or expired. Falling back to password...");
        }

        if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
        {
            Console.WriteLine("No valid username/password provided to fall back to!");
            return IsLoggedIn = false;
        }

        using StringContent content = BuildKemonoLoginPayload(in login);
        try
        {
            Console.WriteLine("Attempting to log in to Kemono...");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/v1/authentication/login", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Logged into Kemono successfully!");    
                return IsLoggedIn = true; 
            }
            else
            {
                Console.WriteLine($"Login failed. Status Code: {response.StatusCode}");
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Details: {errorBody}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request failed: {e.Message}");
        }

        return IsLoggedIn = false;

        static StringContent BuildKemonoLoginPayload(in Login login)
        {
            var credentials = new
            {
                username = login.Username,
                password = login.Password
            };
            string jsonPayload = JsonSerializer.Serialize(credentials);
            return new(jsonPayload, Encoding.UTF8, "application/json");
        }
    }

    public async Task<BaseFavArtist[]> GetFavoriteArtists()
    {
        if (!IsLoggedIn)
        {
            Console.WriteLine("Tried to Get Kemono Favorite Artists without being logged in!");
            return [];
        }

        HttpResponseMessage favResponse = await _httpClient.GetAsync("/api/v1/account/favorites");
        string favBody = await favResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BaseFavArtist[]>(favBody, HelperFunctions.SerializerOptions) ?? [];
    }

    public async Task<BaseFavPost[]> GetFavoritePosts()
    {
        if (!IsLoggedIn)
        {
            Console.WriteLine("Tried to Get Kemono Favorite Posts without being logged in!");
            return [];
        }

        Console.WriteLine("Fetching Kemono Favorite Posts...");
        HttpResponseMessage favResponse = await _httpClient.GetAsync("/api/v1/account/favorites?type=post");
        BaseFavPost[] kemonoPosts = JsonSerializer.Deserialize<BaseFavPost[]>(await favResponse.Content.ReadAsStringAsync(), HelperFunctions.SerializerOptions) ?? [];
        kemonoPosts.AsSpan().Sort(static (a, b) => a.faved_seq.CompareTo(b.faved_seq));
        return kemonoPosts;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
