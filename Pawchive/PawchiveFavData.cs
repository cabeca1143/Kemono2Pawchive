using Kemono2Pawchive.Kemono;

namespace Kemono2Pawchive.Pawchive;

#pragma warning disable CS0649
#pragma warning disable CS8618

class PawchiveFavData : BaseFavArtist
{
    public string public_id;
    public object relation_id;
    public bool ever_imported;
    public int kemono_favorited;
    public object? import_size_cap_gb;
}
