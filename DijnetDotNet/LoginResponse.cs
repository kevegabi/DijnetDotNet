using Newtonsoft.Json;

namespace Dijnet.Net
{
    /// <summary>
    /// Belépési kísérletre válasz
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Sikeres belépést jelző flag
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Kezdő url címe
        /// </summary>
        [JsonProperty("url")]
        public string URL { get; set; }

        /// <summary>
        /// Hiba oka, ha hiba történt
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
