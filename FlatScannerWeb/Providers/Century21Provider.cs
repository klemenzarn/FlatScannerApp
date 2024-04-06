using FlatScannerWeb.Entities;
using HtmlAgilityPack;

namespace FlatScannerWeb.Providers;

public class Century21Provider : FlatProviderBase, IFlatProvider
{
    public bool Enabled => AppSettings.Providers.Century21.Enabled;

    private readonly string _url = "https://c21.si/nepremicnine/prodaja/stanovanje/2-5-sobno,3-sobno/podravska/maribor.html?f%5Bprice%5D=100000-200000&sort=date_added-desc#nepremicnine";

    public Century21Provider(IHttpClientFactory clientFactory, AppSettings appSettings) : base(clientFactory, appSettings)
    {
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsApi()
    {
        using var httpClient = ClientFactory.CreateClient();
        var response = await httpClient.GetAsync(_url);

        if (response.IsSuccessStatusCode)
        {
            var htmlContent = await response.Content.ReadAsStringAsync();
            return ParseData(htmlContent);
        }
        else
        {
            throw new InvalidOperationException($"Century 21 provider api call failed Additional data {response.StatusCode}...");
        }
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsDemo()
    {
        var htmlContent = await File.ReadAllTextAsync("test_files/c21_response.html");
        return ParseData(htmlContent);
    }

    private IEnumerable<FlatEntity> ParseData(string htmlContent)
    {
        var results = new List<FlatEntity>();

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var nodes = doc.DocumentNode.SelectNodes("//div[@class='list']//div[@class='flex item']");

        if (nodes == null)
            return results;

        foreach (var node in nodes)
        {
            var soldNode = node.SelectSingleNode(".//*[contains(@class, 'estate_sold')]");

            if (soldNode != null)
                continue;

            var titleNode = node.SelectSingleNode(".//h3/a");
            var title = titleNode?.InnerText.Trim();

            var subTitleNode = node.SelectSingleNode(".//div[@class='info_list']");
            var subTitle = subTitleNode?.InnerText;

            if (subTitle != null)
            {
                subTitle = subTitle.Replace("\n", "")
                    .Replace("\t", "")
                    .Replace("Stanovanje", "")
                    .Replace("Prodaja", "")
                    .Trim();
            }

            var linkAttribute = titleNode?.Attributes["href"];
            var link = linkAttribute?.Value;

            var priceNode = node.SelectSingleNode(".//div[@class='price']");
            var price = priceNode?.InnerText.Trim();

            var sizeNode = node.SelectSingleNode(".//div[@class='data_list type_2']/div[@class='item']/span[@class='label'][contains(text(), 'Velikost')]/strong");
            var size = sizeNode?.InnerText.Trim();

            results.Add(new FlatEntity
            {
                Name = $"{title} ({subTitle})",
                Price = price ?? string.Empty,
                Size = size ?? string.Empty,
                Link = $"https://c21.si/{link}",
                Provider = "Century 21"
            });
        }

        return results;
    }
}
