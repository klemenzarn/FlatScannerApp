using FlatScannerWeb.Entities;
using HtmlAgilityPack;
using System.IO.Compression;

namespace FlatScannerWeb.Providers;

public class BolhaProvider : FlatProviderBase, IFlatProvider
{
    public bool Enabled => AppSettings.Providers.Bolha.Enabled;

    private readonly string _bolhaUrl = "https://www.bolha.com/prodaja-stanovanja/maribor?price%5Bmin%5D=100000&price%5Bmax%5D=200000&numberOfRooms%5Bmin%5D=twoHalf-room&numberOfRooms%5Bmax%5D=three-rooms&typeOfTransaction=sell";

    public BolhaProvider(IHttpClientFactory clientFactory, AppSettings appSettings) : base(clientFactory, appSettings) { }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsApi()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_bolhaUrl));

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
            throw new InvalidOperationException($"Bolha provider api call failed Additional data {response.StatusCode}...");
        }
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsDemo()
    {
        var htmlContent = await File.ReadAllTextAsync("test_files/bolha_response.html");
        return ParseData(htmlContent);
    }

    private IEnumerable<FlatEntity> ParseData(string htmlContent)
    {
        var results = new List<FlatEntity>();

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'EntityList-item')]");

        if (nodes == null)
            return results;

        foreach (var node in nodes)
        {
            var dataHrefValue = node.GetAttributeValue("data-href", "");

            if (string.IsNullOrWhiteSpace(dataHrefValue))
                continue;

            var titleNode = node.SelectSingleNode(".//*[contains(@class, 'entity-title')]/a");
            var priceNode = node.SelectSingleNode(".//*[contains(@class, 'price-item')]");

            var price = priceNode?.InnerText.Trim();

            if (string.IsNullOrWhiteSpace(price))
                continue;

            results.Add(new FlatEntity
            {
                Name = titleNode?.InnerText ?? string.Empty,
                Price = price ?? string.Empty,
                Size = Constants.UnknownDataString,
                Link = $"https://www.bolha.com{dataHrefValue}",
                Provider = "Bolha"
            });
        }

        return results;
    }
}
