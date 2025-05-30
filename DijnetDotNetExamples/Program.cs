using Dijnet.Net;

namespace DijnetDotNetExample
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Dijnet.NET Implementation (not official) Example (c) 2025. Keve Gábor");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Get username and password from environment variables
            string username = Environment.GetEnvironmentVariable("DIJNET_USERNAME");
            string password = Environment.GetEnvironmentVariable("DIJNET_PASSWORD");

            if (string.IsNullOrEmpty(username))
            {
                Console.Write("Felhasználói név:");
                username = Console.ReadLine();
            }
            if (string.IsNullOrEmpty(password))
            {
                Console.Write("Jelszó:");
                password = Console.ReadLine();
            }

            // Initialize the service
            var service = new DijnetService();

            // Login to the service
            try
            {
                Console.WriteLine("Belépés a Dijnet rendszerébe.");
                await service.LoginAsync(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a login során: {ex.Message}");
                return;
            }

            // Get basic providers informations
            Console.WriteLine("Alap szolgáltatói lista");
            try
            {
                var providers = await service.GetProvidersAsync();
                Console.WriteLine(string.Join(", ", providers));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a szolgáltató lekérdezés közben: {ex.Message}");
                return;
            }

            //Get detailed providers informations
            Console.WriteLine("Részletes szolgáltatói lista");
            try
            {
                var providerInformations = await service.GetProviderDetailedInformationAsync();
                foreach (var pi in providerInformations)
                {
                    Console.WriteLine($"{pi.InvoiceProviderName} - {pi.AliasName} - {pi.Alias} - {pi.ProviderId} - {pi.RegProviderId} - {pi.UserId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a szolgáltató lekérdezés közben: {ex.Message}");
                return;
            }


            // Define the query for invoices
            var query = new InvoicesQuery
            {
                From = DateTime.UtcNow.AddDays(-30),
                To = DateTime.UtcNow,
            };

            // Get archived invoices
            List<Invoice> invoices;
            Console.WriteLine($"Számlák lekérdezése a {query.From} -tól {query.To} időszakig");
            try
            {
                invoices = await service.GetInvoicesAsync(query);
                if (invoices != null)
                {
                    Console.WriteLine(string.Join("\n", invoices?.Select(i => i.ToString())));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a számlák lekérdezése közben: {ex.Message}");
                return;
            }

            if (invoices != null && invoices.Count > 0)
            {
                Console.WriteLine("Számla fájlok letöltése");
                if (!Path.Exists("invoices")) Directory.CreateDirectory("invoices");
                // Download invoices && xml files
                foreach (var invoice in invoices)
                {
                    try
                    {
                        Console.WriteLine($"{invoice.Provider} - {invoice.InvoiceID} letöltése megkezdődött");
                        string pdfPath = Path.Combine("invoices", invoice.InvoiceID.Replace("/", "_") + ".pdf");
                        string xmlPath = Path.Combine("invoices", invoice.InvoiceID.Replace("/", "_") + ".xml");
                        await service.DownloadInvoiceAsync(invoice, pdfPath, xmlPath);
                        Console.WriteLine($"{pdfPath} helyre letöltve");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Hiba történt a számla fájlok (PDF, XML) letöltése közben: {ex.Message}");
                    }
                }
            }
            else
                Console.WriteLine("Az adott időszakban nem voltak számlák");
        }
    }
}
