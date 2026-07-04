using System.Text.Json.Serialization;

namespace Kemono2Pawchive;

internal class ConfigFile
{
    [JsonPropertyName("kemono_address")]
    public string KemonoAddress = "https://kemono.cr";
    [JsonPropertyName("pawchive_address")]
    public string PawchiveAddress = "https://pawchive.pw";
    [JsonPropertyName("credentials")]
    public Credentials Credentials = null!;
    [JsonPropertyName("services")]
    public string[] Services = [];
    [JsonPropertyName("timeout_MS")]
    public int RequestTimeout = 1500;

    internal static void WriteDefault()
    {
        const string defaultConfigJson = @"{
   ""kemono_address"":""https://kemono.cr"",
   ""pawchive_address"":""https://pawchive.pw"",
   ""credentials"":{
      ""kemono"":{
         ""username"":"""",
         ""password"":"""",
         ""session_cookie"":"""",
         ""cf_clearance"":""""
      },
      ""pawchive"":{
         ""username"":"""",
         ""password"":"""",
         ""session_cookie"":"""",
         ""cf_clearance"":""""
      }
   },
   ""services"":[
      ""patreon"",
      ""fanbox""
   ],
   ""timeout_MS"":1500
}";

        File.WriteAllText("config.json", defaultConfigJson);
    }
}
