using System;

namespace Dijnet.Net
{
    /// <summary>
    /// Számla lekérdezéshez paraméterek
    /// </summary>
    public class InvoicesQuery
    {
        /// <summary>
        /// Szolgáltató egyedi azonosítója
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Kiállító egyedi azonosítója
        /// </summary>
        public string IssuerID { get; set; }

        /// <summary>
        /// Számla kiállításának kezdeti dátuma
        /// </summary>
        public DateTime From { get; set; }

        /// <summary>
        /// Számla kiállításának záró dátuma
        /// </summary>
        public DateTime To { get; set; }
    }
}
