using FlatScannerWeb.Entities;
using HtmlAgilityPack;
using System.IO.Compression;
using System.Net;

namespace FlatScannerWeb.Providers;

public class NepremicnineProvider : FlatProviderBase, IFlatProvider
{
    public bool Enabled => AppSettings.Providers.Nepremicnine.Enabled;

    private readonly string _url = "https://www.nepremicnine.net/oglasi-prodaja/podravska/maribor/stanovanje/2.5-sobno,3-sobno/cena-od-100000-do-200000-eur,velikost-od-50-do-100-m2/?nadst%5B0%5D=vsa&nadst%5B1%5D=vsa";

    public NepremicnineProvider(IHttpClientFactory clientFactory, AppSettings appSettings) : base(clientFactory, appSettings)
    {
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsApi()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_url));
        
        request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
        request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
        request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

        using var httpClient = ClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(decompressedStream);

            var htmlContent = await streamReader.ReadToEndAsync();
            return ParseData(htmlContent);
        }
        else
        {
            throw new InvalidOperationException($"Nepremicnine provider api call failed Additional data {response.StatusCode}...");
        } 
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsDemo()
    {
        var htmlContent = await File.ReadAllTextAsync("test_files/nepremicnine_response.html");
        return ParseData(htmlContent);
    }

    private IEnumerable<FlatEntity> ParseData(string htmlContent)
    {
        var results = new List<FlatEntity>();

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var nodes = doc.DocumentNode.SelectNodes("//div[@class='seznam']//*[contains(@class, 'property-box property-normal')]");

        if (nodes == null)
            return results;

        foreach (var node in nodes)
        {
            var linkNode = node.SelectSingleNode(".//a[@class='url-title-d']");
            var link = linkNode?.Attributes["href"]?.Value;

            var titleNode = linkNode?.SelectSingleNode(".//h2");
            var title = titleNode?.InnerText.Trim();

            var subTitleNode = node.SelectSingleNode(".//span[@class='tipi']");
            if (subTitleNode != null && title != null)
                title += $" ({subTitleNode.InnerText.Trim()})";

            var priceNode = node.SelectSingleNode(".//h6");
            var price = priceNode?.InnerText.Trim();

            var sizeNode = node.SelectSingleNode(".//li[contains(img/@src, 'velikost.svg')]/text()");
            var size = sizeNode?.InnerText.Trim();

            results.Add(new FlatEntity
            {
                Name = title ?? string.Empty,
                Price = price ?? string.Empty,
                Size = size ?? string.Empty,
                Link = link ?? Constants.UnknownDataString,
                Provider = "Nepremičnine"
            });
        }

        return results;
    }
}
