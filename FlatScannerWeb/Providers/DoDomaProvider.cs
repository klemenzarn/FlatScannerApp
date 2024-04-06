using FlatScannerWeb.Entities;
using HtmlAgilityPack;

namespace FlatScannerWeb.Providers;

public class DoDomaProvider : FlatProviderBase, IFlatProvider
{
    public DoDomaProvider(IHttpClientFactory clientFactory, AppSettings appSettings) : base(clientFactory, appSettings)
    {
    }

    public bool Enabled => AppSettings.Providers.DoDoma.Enabled;

    public override async Task<IEnumerable<FlatEntity>> GetFlatsApi()
    {
        string url = "https://www.dodoma.si/oglasi";
        string cookieValue = "cc_cookie_accept=cc_cookie_accept; PHPSESSID=k1sgm3ap9depmnl2q4tp4p06e5; lng=si; referer=https%3A%2F%2Fwww.google.com%2F; filter_segment=%7B%22filter%22%3A%22%22%7D; filter=%7B%22offer_type%22%3A%221%22%2C%22property_type%22%3A%225%22%2C%22property_subtype%22%3A%5B%2216%22%2C%2217%22%5D%2C%22location%22%3A%5B%222%3A%3A41%3A%3A0%22%5D%2C%22size_min%22%3A%22%22%2C%22size_max%22%3A%22%22%2C%22price_min%22%3A%22100.000%22%2C%22price_max%22%3A%22200.000%22%2C%22search%22%3A%221%22%2C%22searched_view%22%3A%22%22%2C%22option%22%3A%22advanced%22%7D; filters=%7B%22https%3A%5C%2F%5C%2Fwww.dodoma.si%5C%2Foglasi%22%3A%22%7B%5C%22offer_type%5C%22%3A%5C%221%5C%22%2C%5C%22property_type%5C%22%3A%5C%225%5C%22%2C%5C%22property_subtype%5C%22%3A%5B%5C%2216%5C%22%2C%5C%2217%5C%22%5D%2C%5C%22location%5C%22%3A%5B%5C%222%3A%3A41%3A%3A0%5C%22%5D%2C%5C%22size_min%5C%22%3A%5C%22%5C%22%2C%5C%22size_max%5C%22%3A%5C%22%5C%22%2C%5C%22price_min%5C%22%3A%5C%22100.000%5C%22%2C%5C%22price_max%5C%22%3A%5C%22200.000%5C%22%2C%5C%22search%5C%22%3A%5C%221%5C%22%2C%5C%22searched_view%5C%22%3A%5C%22%5C%22%2C%5C%22option%5C%22%3A%5C%22advanced%5C%22%7D%22%7D";

        using var client = ClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookieValue);

        var response = await client.PostAsync(url, null);

        if (response.IsSuccessStatusCode)
        {
            var htmlContent = await response.Content.ReadAsStringAsync();
            return ParseData(htmlContent);
        }
        else
        {
            throw new InvalidOperationException($"DoDoma provider api call failed Additional data {response.StatusCode}...");
        }
    }

    public override async Task<IEnumerable<FlatEntity>> GetFlatsDemo()
    {
        var htmlContent = await File.ReadAllTextAsync("test_files/dodoma_response.html");
        return ParseData(htmlContent);
    }

    private IEnumerable<FlatEntity> ParseData(string htmlContent)
    {
        var results = new List<FlatEntity>();

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        var nodes = doc.DocumentNode.SelectNodes("//div[@class='item-list']//*[contains(@class, 'row-wrapper')]");

        if (nodes == null)
            return results;

        foreach (var node in nodes)
        {

            var linkNode = node.SelectSingleNode(".//a[contains(@class, 'about')]");
            var link = linkNode?.Attributes["href"]?.Value;

            var titleNode = linkNode?.SelectSingleNode(".//h2");
            var title = titleNode?.InnerText.Trim();

            var subTitleNode = node.SelectSingleNode(".//div[@class='description']");
            var subTitle = subTitleNode?.InnerText.Trim();

            var priceNode = node.SelectSingleNode(".//p[@class='price']/strong");
            var price = priceNode?.InnerText.Trim();

            var sizeNode = node.SelectSingleNode(".//p[@class='size']/span");
            var size = sizeNode?.InnerText.Trim();

            results.Add(new FlatEntity
            {
                Name = $"{subTitle} ({title})",
                Price = price ?? string.Empty,
                Size = size ?? string.Empty,
                Link = link ?? string.Empty,
                Provider = "Do doma"
            });
        }

        return results;
    }

}
