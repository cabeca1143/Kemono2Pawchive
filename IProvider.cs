using Kemono2Pawchive.Kemono;
using static Kemono2Pawchive.Credentials;

namespace Kemono2Pawchive;

internal interface IProvider : IDisposable
{
    bool IsLoggedIn { get; }
    Task<bool> Login(Login login);
}

internal interface IReadProvider<T> : IProvider
{
    Task<T[]> GetFavoriteArtists();
    Task<BaseFavPost[]> GetFavoritePosts();
}

internal interface IWriteProvider : IProvider
{
    Task SetFavoriteArtist(BaseFavArtist toAdd);
    Task SetFavoritePost(BaseFavPost post);
}

internal interface IReadWriteProvider<T> : IReadProvider<T>, IWriteProvider
{
}
