using System.Text.Json.Serialization;

namespace Kemono2Pawchive;

#pragma warning disable CS0649

internal class Credentials
{
    [JsonPropertyName("kemono")]
    public Login Kemono;
    [JsonPropertyName("pawchive")]
    public Login Pawchive;

    internal struct Login
    {
        [JsonPropertyName("username")]
        public string? Username;
        [JsonPropertyName("password")]
        public string? Password;
        [JsonPropertyName("session_cookie")]
        public string? Cookie;
        [JsonPropertyName("cf_clearance")]
        public string? CloudflareClearance;

        internal readonly bool HasValidAuth()
        {
            bool hasPassword = !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
            bool hasCookie = !string.IsNullOrEmpty(Cookie);
            return hasPassword || hasCookie;
        }
    }
}
