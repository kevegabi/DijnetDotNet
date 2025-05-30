using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dijnet.Net
{
    public interface IDijnetService
    {
        /// <summary>
        /// Login indítás
        /// </summary>
        /// <param name="username">felhasználói név<example>tesztelek01</example> </param>
        /// <param name="password">jelszó kódolás nélkül <example>kiskutya</example></param>
        /// <returns></returns>
        Task LoginAsync(string username, string password);

        /// <summary>
        /// Szolgáltatók listájának lekérdezése <seealso cref="GetProviderDetailedInformationAsync"/>
        /// </summary>
        /// <returns>Rögzített szolgáltatók listája szövegesen</returns>
        Task<List<string>> GetProvidersAsync();

        /// <summary>
        /// Szolgáltatók részletes listája <seealso cref="GetProvidersAsync"/>
        /// </summary>
        /// <returns>Szolgáltatók részletes adatai <see cref="Provider"/></returns>
        Task<List<Provider>> GetProviderDetailedInformationAsync();

        /// <summary>
        /// Számlák lekérdezése
        /// </summary>
        /// <param name="query">Lekérdezés paraméterei <see cref="InvoicesQuery"/></param>
        /// <returns></returns>
        Task<List<Invoice>> GetInvoicesAsync(InvoicesQuery query);

        /// <summary>
        /// Számla letöltése
        /// </summary>
        /// <param name="invoice">Számla adatai <see cref="Invoice"/></param>
        /// <param name="pdf">PDF fájl neve</param>
        /// <param name="xml">XML fájl neve</param>
        /// <returns></returns>
        Task DownloadInvoiceAsync(Invoice invoice, string pdf, string xml);
    }
}
