using System;

namespace Dijnet.Net
{
    /// <summary>
    /// Számla információ
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Számla egyedi azonosítója
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Szolgáltató neve
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Kiállító egyedi azonosítója
        /// </summary>
        public string IssuerID { get; set; }

        /// <summary>
        /// Számla egyedi azonosítója
        /// </summary>
        public string InvoiceID { get; set; }

        /// <summary>
        /// számla kiállítás dátuma
        /// </summary>
        public DateTime DateOfIssue { get; set; }

        /// <summary>
        /// Teljes összeg
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Fizetési határidő
        /// </summary>
        public DateTime PaymentDeadline { get; set; }

        /// <summary>
        /// Fizetendő összeg
        /// </summary>
        public int Payable { get; set; }

        /// <summary>
        /// Aktuális státusz
        /// </summary>
        public string Status { get; set; }

        public override string ToString()
        {
            return $"{ID} - {Provider} - {Total} - {DateOfIssue} - {Status}";
        }
    }
}
