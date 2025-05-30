using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if NETCOREAPP
using System.Web;
#endif

namespace Dijnet.Net
{
    /// <summary>
    /// Dijnet.Net 
    /// </summary>
    public class DijnetService : IDijnetService
    {
        private readonly HttpClient _client;
        private readonly string _baseURL = "https://www.dijnet.hu";
        /// <summary>
        /// Current token for communication
        /// </summary>
        private static string CurrentToken { get; set; }

        /// <summary>
        /// Provider list
        /// </summary>
        public static List<Provider> KnownProviders { get; private set; }

        /// <summary>
        /// Unpaid invoice list
        /// </summary>
        public List<Invoice> UnpaidInvoices { get; private set; }

        /// <summary>
        /// Dijnet.NET service
        /// </summary>
        public DijnetService()
        {
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };
            _client = new HttpClient(handler);
        }

        /// <summary>
        /// Internal login
        /// </summary>
        /// <param name="username">dijnet username</param>
        /// <param name="password">dijnet password</param>
        /// <returns></returns>
        private async Task<LoginResponse> InternalLoginAsync(string username, string password)
        {
            var payload = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

            var response = await _client.PostAsync($"{_baseURL}/ekonto/login/login_check_ajax", payload);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponse>(responseBody);
            return result;
        }

        /// <summary>
        /// Produce HtmlDocument from response body
        /// </summary>
        /// <param name="response">the current HTTP response body</param>
        /// <returns>HtmlDocument</returns>
        private static async Task<HtmlDocument> GetHtmlDocumentAsync(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStreamAsync();
            var document = new HtmlDocument();
            document.Load(responseBody, Encoding.GetEncoding("iso-8859-2"));
            return document;
        }

        /// <summary>
        /// Get the actual token from downloaded document
        /// </summary>
        /// <param name="document">HtmlDocument</param>
        /// <returns>the actual token</returns>
        public string GetToken(HtmlDocument document)
        {
            try
            {
                return document.DocumentNode.SelectSingleNode("//input[@name='vfw_token']").GetAttributeValue("value", "");
            }
            catch 
            {
                return null;
            }
        }

        /// <summary>
        /// Download and process the main dijnet web page
        /// </summary>
        /// <param name="mainURL">the main URL from login</param>
        /// <returns></returns>
        private async Task VisitMainAsync(string mainURL)
        {
            var response = await _client.GetAsync($"{_baseURL}{mainURL}");
            response.EnsureSuccessStatusCode();
            var document = await GetHtmlDocumentAsync(response);
            CurrentToken = GetToken(document);
            UnpaidInvoices = GetUnpaidInvoices(document);
        }

        /// <summary>
        /// Login to Dijnet web page
        /// </summary>
        /// <param name="username">dijnet username</param>
        /// <param name="password">dijnet password</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task LoginAsync(string username, string password)
        {
            var status = await InternalLoginAsync(username, password);
            if (!status.Success)
            {
                throw new Exception($"Unable to login: {status.Error}");
            }

            await VisitMainAsync(status.URL);
        }

        /// <summary>
        /// Get the simple providers informations
        /// </summary>
        /// <returns>the provider names</returns>
        public async Task<List<string>> GetProvidersAsync()
        {
            var response = await _client.GetAsync($"{_baseURL}/ekonto/control/szamla_search");
            response.EnsureSuccessStatusCode();

            var document = await GetHtmlDocumentAsync(response);

            var providers = new List<string>();
            var form = document.DocumentNode.SelectSingleNode("//div[@id='content_bs']//div");

            var script = form.InnerHtml;
            var matches = Regex.Matches(script, @"sopts.add\('(.+?)'\)");
            foreach (Match match in matches)
            {
                providers.Add(match.Groups[1].Value);
            }

            CurrentToken = GetToken(document);

            return providers;
        }

        /// <summary>
        /// Get full Provider informations from HtmlDocument
        /// </summary>
        /// <param name="doc">the downloaded HtmlDocument</param>
        /// <returns>Full provider information list</returns>
        private static List<Provider> GetProviderInformation(HtmlDocument doc)
        {
            // A teljes HTML-ből kinyerjük a ropts változót tartalmazó JavaScript blokkot
            var scriptNodes = doc.DocumentNode.SelectNodes("//script");
            string roptsJson = null;
            foreach (var match in from script in scriptNodes
                                  where script.InnerText.Contains("var ropts = [")
                                  let match = Regex.Match(script.InnerText, @"var ropts\s*=\s*(\[\{.*?\}\]);", RegexOptions.Singleline)
                                  where match.Success
                                  select match)
            {
                roptsJson = match.Groups[1].Value;
            }

            if (roptsJson != null)
            {
                // A JSON string dekódolása
                return JsonConvert.DeserializeObject<List<Provider>>(roptsJson);
            }
            else
            {
                Console.WriteLine("Nem található ropts JSON.");
            }
            return new List<Provider>();
        }

        /// <summary>
        /// Build the payload
        /// </summary>
        /// <param name="Provider">Provider Id</param>
        /// <param name="IssuerId">Issuer Id</param>
        /// <param name="FromDate">From date</param>
        /// <param name="ToDate">To date</param>
        /// <returns>payload</returns>
        private static FormUrlEncodedContent CreatePayload(string Provider, string IssuerId, DateTime? FromDate, DateTime? ToDate)
        {
            return new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("vfw_form", "szamla_search_submit"),
            new KeyValuePair<string, string>("vfw_token", CurrentToken),
            new KeyValuePair<string, string>("vfw_coll", "szamla_search_params"),
            #if NET471_OR_GREATER
            new KeyValuePair<string, string>("szlaszolgnev", Provider??WebUtility.UrlEncode(Provider)),
            #endif
            #if NETCOREAPP
            new KeyValuePair<string, string>("szlaszolgnev", Provider??HttpUtility.UrlEncode(Provider)),
            #endif
            new KeyValuePair<string, string>("regszolgid", IssuerId??IssuerId),
            new KeyValuePair<string, string>("datumtol", FromDate.HasValue? FromDate.Value.ToString("yyyy-MM-dd"):""),
            new KeyValuePair<string, string>("datumig", ToDate.HasValue? ToDate.Value.ToString("yyyy-MM-dd"):"")
            });
        }

        /// <summary>
        /// Get detailed providers informations
        /// </summary>
        /// <returns>list of detailed provider informations</returns>
        public async Task<List<Provider>> GetProviderDetailedInformationAsync()
        {
            var query = new InvoicesQuery()
            {
            };

            var payload = CreatePayload(query.Provider, query.IssuerID, query.From, query.To);

            var response = await _client.PostAsync($"{_baseURL}/ekonto/control/szamla_search_submit", payload);
            response.EnsureSuccessStatusCode();

            var document = await GetHtmlDocumentAsync(response);

            return GetProviderInformation(document);
        }

        /// <summary>
        /// Get the unpaided invoices list
        /// </summary>
        /// <param name="document">HtmlDocument</param>
        /// <returns>Invoice list</returns>
        public List<Invoice> GetUnpaidInvoices(HtmlDocument document)
        {
            var invoices = new List<Invoice>();
            var tbodyNode = document.DocumentNode.SelectSingleNode("//table[@class='table table-hover mb-0']/tbody");
            if (tbodyNode != null)
            {
                var rows = tbodyNode.SelectNodes(".//tr[starts-with(@id, 'r_')]");

                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells != null && cells.Count() >= 8)
                        {
                            Invoice invoice = new Invoice();

                            invoice.ID = row.GetAttributeValue("id", "").Replace("r_", "");
                            invoice.Provider = cells[0].InnerText.Trim();

                            invoice.IssuerID = cells[1].InnerText.Trim();
                            invoice.InvoiceID = cells[2].InnerText.Trim();

                            string dateOfIssueString = cells[3].InnerText.Trim();
                            if (DateTime.TryParseExact(dateOfIssueString, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime issueDate))
                            {
                                invoice.DateOfIssue = issueDate;
                            }
                            else
                            {
                                invoice.DateOfIssue = DateTime.MinValue;
                            }
                            string totalString = cells[4].InnerText.Replace(" ", "").Replace("Ft", "").Trim();
                            if (int.TryParse(totalString, out int total))
                            {
                                invoice.Total = total;
                            }

                            string paymentDeadlineString = cells[5].InnerText.Trim();
                            if (DateTime.TryParseExact(paymentDeadlineString, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadlineDate))
                            {
                                invoice.PaymentDeadline = deadlineDate;
                            }
                            else
                            {
                                invoice.PaymentDeadline = DateTime.MinValue;
                            }
                            string payableString = cells[6].InnerText.Replace(" ", "").Replace("Ft", "").Replace("*", "").Trim();
                            if (int.TryParse(payableString, out int payable))
                            {
                                invoice.Payable = payable;
                            }

                            var statusNode = cells[7].SelectSingleNode(".//a") ?? cells[7];
                            invoice.Status = statusNode.InnerText.Trim();

                            invoices.Add(invoice);
                        }
                    }
                }
            }
            return invoices;
        }

        /// <summary>
        /// Get the archived invoice list
        /// </summary>
        /// <param name="query">invoice query</param>
        /// <returns>invoices list</returns>
        public async Task<List<Invoice>> GetInvoicesAsync(InvoicesQuery query)
        {
            var payload = CreatePayload(query.Provider, query.IssuerID, query.From, query.To);

            var response = await _client.PostAsync($"{_baseURL}/ekonto/control/szamla_search_submit", payload);
            response.EnsureSuccessStatusCode();

            var document = await GetHtmlDocumentAsync(response);

            var invoices = new List<Invoice>();

            var table = document.DocumentNode.SelectSingleNode("//div[@id='szamla_table_cont']//table");
            var rows = table.SelectNodes(".//tbody/tr");

            if (rows == null) return null;
            foreach (var row in rows)
            {
                var invoice = new Invoice();
                var cells = row.SelectNodes("td");

                invoice.ID = row.GetAttributeValue("id", "").Split('_')[1];
                invoice.Provider = cells[0].InnerText.Trim();
                invoice.IssuerID = cells[1].InnerText.Trim();
                invoice.InvoiceID = cells[2].InnerText.Trim();
                invoice.DateOfIssue = DateTime.ParseExact(cells[3].InnerText, "yyyy.MM.dd", null);
                invoice.Total = int.Parse(string.Concat(cells[4].InnerText.Where(char.IsNumber)));
                invoice.PaymentDeadline = DateTime.ParseExact(cells[5].InnerText, "yyyy.MM.dd", null);
                invoice.Payable = int.Parse(string.Concat(cells[6].InnerText.Where(char.IsNumber)));
                invoice.Status = cells[7].InnerText;

                invoices.Add(invoice);
            }

            return invoices;
        }

        /// <summary>
        /// Download file from url
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="filename">local filename with path</param>
        /// <returns></returns>
        private async Task DownloadFileAsync(string url, string filename)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        /// <summary>
        /// Download invoice files (PDF, XML)
        /// </summary>
        /// <param name="invoice">invoice</param>
        /// <param name="pdf">path of PDF</param>
        /// <param name="xml">path of XML</param>
        /// <returns></returns>
        public async Task DownloadInvoiceAsync(Invoice invoice, string pdf, string xml)
        {
            var response = await _client.GetAsync($"{_baseURL}/ekonto/control/szamla_select?vfw_coll=szamla_list&vfw_rowid={invoice.ID}");
            response.EnsureSuccessStatusCode();
            response.Dispose();

            response = await _client.GetAsync($"{_baseURL}/ekonto/control/szamla_letolt");
            response.EnsureSuccessStatusCode();
            response.Dispose();

            if (!string.IsNullOrEmpty(pdf))
            {
                await DownloadFileAsync($"{_baseURL}/ekonto/control/szamla_pdf", pdf);
            }

            if (!string.IsNullOrEmpty(xml))
            {
                await DownloadFileAsync($"{_baseURL}/ekonto/control/szamla_xml", xml);
            }

            response = await _client.GetAsync($"{_baseURL}/ekonto/control/szamla_list");
            response.EnsureSuccessStatusCode();
            response.Dispose();
        }
    }
}