using Kemono2Pawchive.Kemono;
using Kemono2Pawchive.Pawchive;
using System.Diagnostics;

namespace Kemono2Pawchive;

internal class Kemono2PawchiveManager : IDisposable
{
    private readonly int FavTimeoutTimeMS;

    private readonly IReadProvider<BaseFavArtist> KemonoClient;
    private readonly IReadWriteProvider<PawchiveFavData> PawchiveClient;

    public bool IsKemonoLoggedIn => KemonoClient.IsLoggedIn;
    public bool IsPawchiveLoggedIn => PawchiveClient.IsLoggedIn;

    private readonly HashSet<string> Services;
    private Kemono2PawchiveManager(string kemonoURL, string pawchiveURL, string[] services, int timeout)
    {
        FavTimeoutTimeMS = timeout;
        Services = [.. services];
        KemonoClient = new KemonoProvider(kemonoURL);
        PawchiveClient = new PawchiveProvider(pawchiveURL);
    }

    public static async Task<Kemono2PawchiveManager?> Instantiate(ConfigFile cfg)
    {
        Kemono2PawchiveManager instance = new(cfg.KemonoAddress, cfg.PawchiveAddress, cfg.Services, cfg.RequestTimeout);

        Task<bool> kemonoLoginTask = instance.KemonoClient.Login(cfg.Credentials.Kemono);
        Task<bool> pawchiveLoginTask = instance.PawchiveClient.Login(cfg.Credentials.Pawchive);

        if (!await kemonoLoginTask)
        {
            Console.WriteLine("Failed to login on Kemono");
            return null;
        }
        if (!await pawchiveLoginTask)
        {
            Console.WriteLine("Failed to login on Pawchive");
            return null;
        }

        return instance;
    }

    public async Task ProcessArtistsFromKemono2Pawchive()
    {
        if (!IsKemonoLoggedIn)
        {
            Console.WriteLine("Kemono is not Logged In!");
            return;
        }
        if (!IsPawchiveLoggedIn)
        {
            Console.WriteLine("Pawchive is not Logged In!");
            return;
        }

        Task<PawchiveFavData[]> pawchiveDataTask = PawchiveClient.GetFavoriteArtists();
        Task<BaseFavArtist[]> kemonoDataTask = KemonoClient.GetFavoriteArtists();

        PawchiveFavData[] pawchiveData = await pawchiveDataTask;
        HashSet<string> filter = [.. pawchiveData.Select(x => x.id)];
        filter.RemoveWhere(x => string.IsNullOrEmpty(x));

        foreach (BaseFavArtist kemonoFav in await kemonoDataTask)
        {
            if (Services.Count > 0 && !Services.Contains(kemonoFav.service))
            {
                continue;
            }
            if (string.IsNullOrEmpty(kemonoFav.id)) continue;
            if (filter.Contains(kemonoFav.id))
            {
                Console.WriteLine($"Creator {kemonoFav.name} already faved, skipping...");
                continue;
            }

            await PawchiveClient.SetFavoriteArtist(kemonoFav);

            await Task.Delay(FavTimeoutTimeMS);
        }
    }

    public async Task ProcessPostsFromKemono2Pawchive()
    {
        if (!IsKemonoLoggedIn)
        {
            Console.WriteLine("Kemono is not Logged In!");
            return;
        }
        if (!IsPawchiveLoggedIn)
        {
            Console.WriteLine("Pawchive is not Logged In!");
            return;
        }

        Task<BaseFavPost[]> kemonoPosts = KemonoClient.GetFavoritePosts();
        BaseFavPost[] pawchivePosts = await PawchiveClient.GetFavoritePosts();
        HashSet<ValueTuple<string, string, string>> set = [.. pawchivePosts.Select(x => new ValueTuple<string, string, string>(x.service, x.user, x.id))];
        BaseFavPost[] kemonoFavs = await kemonoPosts;

        Console.WriteLine("Starting Pocessing...");
        foreach (BaseFavPost postToFav in kemonoFavs)
        {
            if (Services.Count > 0 && !Services.Contains(postToFav.service))
            {
                continue;
            }
            ValueTuple<string, string, string> baseTuple = (postToFav.service, postToFav.user, postToFav.id);
            if (set.Contains(baseTuple))
            {
                Console.WriteLine($"Post {postToFav.title} already faved, skipping...");
                continue;
            }
            await PawchiveClient.SetFavoritePost(postToFav);
            await Task.Delay(FavTimeoutTimeMS);
        }
    }

    public void Dispose()
    {
        KemonoClient.Dispose();
        PawchiveClient.Dispose();
    }
}
