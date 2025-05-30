using Newtonsoft.Json;

namespace Dijnet.Net
{
    /// <summary>
    /// Részletes szolgáltatói adatok
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Egyedi név
        /// </summary>
        [JsonProperty("aliasnev")]
        public string AliasName { get; set; }

        /// <summary>
        /// Számla szolgáltató neve
        /// </summary>
        [JsonProperty("szlaszolgnev")]
        public string InvoiceProviderName { get; set; }

        /// <summary>
        /// Számla szolgáltató egyedi azonosítója
        /// </summary>
        [JsonProperty("regszolgid")]
        public long RegProviderId { get; set; }

        /// <summary>
        /// Ügyfél egyedi azonosítój a számlaszolgáltatónál
        /// </summary>
        [JsonProperty("ugyfelazon")]
        public string UserId { get; set; }

        /// <summary>
        /// Státusz csoport ???
        /// </summary>
        [JsonProperty("statusgrp")]
        public int StatusGrp { get; set; }

        /// <summary>
        /// Szolgáltató egyedi azonosítója
        /// </summary>
        [JsonProperty("szolgid")]
        public int ProviderId { get; set; }

        /// <summary>
        /// Felhasználó által adott név
        /// </summary>
        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}
