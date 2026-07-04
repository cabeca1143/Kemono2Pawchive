using Kemono2Pawchive.Kemono;
using System.Net;
using System.Text.Json;
using static Kemono2Pawchive.Credentials;

namespace Kemono2Pawchive.Pawchive;

internal class PawchiveProvider : IReadWriteProvider<PawchiveFavData>
{
    private readonly string _baseAddress;

    private HttpClient _httpClient;
    private CookieContainer _cookieContainer;
    public bool IsLoggedIn { get; private set; }

    public PawchiveProvider(string baseAddress)
    {
        _baseAddress = baseAddress;
        _httpClient = HelperFunctions.SetupHttpClient(_baseAddress, out _cookieContainer);
    }

    public async Task<bool> Login(Login login)
    {
        if (!string.IsNullOrEmpty(login.Cookie))
        {
            Console.WriteLine("Pawchive session cookie found. Testing cookie...");
            _cookieContainer.Add(new Uri(_baseAddress), new Cookie("session", login.Cookie));
            if(!string.IsNullOrEmpty(login.CloudflareClearance))
            {
                _cookieContainer.Add(new Uri(_baseAddress), new Cookie("cf_clearance", login.CloudflareClearance));
            }

            HttpResponseMessage testResponse = await _httpClient.GetAsync("/api/v1/account/favorites?type=artist");
            if (testResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Logged in to Pawchive successfully via Cookie!");
                return IsLoggedIn = true;
            }

            Console.WriteLine("Pawchive Cookie was invalid or expired. Falling back to password...");
        }

        if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
        {
            Console.WriteLine("No valid username/password provided for Pawchive to fall back to!");
            return IsLoggedIn = false;
        }

        using FormUrlEncodedContent content = BuildPawchiveLoginPayload(in login);
        try
        {
            Console.WriteLine("Attempting to log in to Pawchive...");

            HttpResponseMessage response = await _httpClient.PostAsync("/account/login", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Logged into Pawchive successfully!");
                return IsLoggedIn = true;
            }
            else
            {
                Console.WriteLine($"Login to Pawchive failed. Status Code: {response.StatusCode}");
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Details: {errorBody}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request to Pawchive failed: {e.Message}");
        }

        return IsLoggedIn = false;

        static FormUrlEncodedContent BuildPawchiveLoginPayload(in Login login)
        {
            var formData = new Dictionary<string, string>
                {
                    { "username", login.Username ?? ""},
                    { "password", login.Password ?? ""}
                };

            return new FormUrlEncodedContent(formData);
        }
    }

    public async Task<PawchiveFavData[]> GetFavoriteArtists()
    {
        if (!IsLoggedIn)
        {
            Console.WriteLine("Tried to Get Pawchive Favorites without being logged in!");
            return [];
        }

        HttpResponseMessage favResponse = await _httpClient.GetAsync("/api/v1/account/favorites?type=artist");
        string favBody = await favResponse.Content.ReadAsStringAsync();

        PawchiveFavData[]? data = JsonSerializer.Deserialize<PawchiveFavData[]>(favBody, HelperFunctions.SerializerOptions);

        if (data is null)
        {
            Console.WriteLine("Failed to get fav data from Pawchive!");
            return [];
        }
        return data;
    }

    public async Task SetFavoriteArtist(BaseFavArtist toFav)
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"/api/v1/favorites/creator/{toFav.service}/{toFav.id}", null);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Faved {toFav.service} {toFav.name}");
        }
        else
        {
            Console.WriteLine($"Faving {toFav.service} {toFav.name} returned an error! (id: {toFav.id}");
        }
    }

    public void Dispose() 
    {
        _httpClient.Dispose();
    }

    public async Task<BaseFavPost[]> GetFavoritePosts()
    {
        if (!IsLoggedIn)
        {
            Console.WriteLine("Tried to Get Pawchive Favorite Posts without being logged in!");
            return [];
        }

        Console.WriteLine("Fetching Pawchive Favorite Posts...");
        HttpResponseMessage favResponse = await _httpClient.GetAsync("/api/v1/account/favorites?type=post");
        BaseFavPost[] pawchivePosts = JsonSerializer.Deserialize<BaseFavPost[]>(await favResponse.Content.ReadAsStringAsync(), HelperFunctions.SerializerOptions) ?? [];
        pawchivePosts.AsSpan().Sort(static (a, b) => a.faved_seq.CompareTo(b.faved_seq));
        return pawchivePosts;
    }

    public async Task SetFavoritePost(BaseFavPost toFav)
    {
        HttpResponseMessage response = await _httpClient.PostAsync($"/api/v1/favorites/post/{toFav.service}/{toFav.user}/{toFav.id}", null);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Faved {toFav.title}");
        }
        else
        {
            Console.WriteLine($"Faving {toFav.title} returned an error! (id: {toFav.id}");
        }
    }
}
