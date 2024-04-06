using FlatScannerWeb.Entities;
using HtmlAgilityPack;

namespace FlatScannerWeb.Providers
{
    public class GaleaProvider : FlatProviderBase, IFlatProvider
    {
        public bool Enabled => AppSettings.Providers.Galea.Enabled;

        public GaleaProvider(IHttpClientFactory clientFactory, AppSettings appSettings) : base(clientFactory, appSettings) { }

        public override async Task<IEnumerable<FlatEntity>> GetFlatsApi()
        {
            var url = "https://www.galea.si/rezultati-iskanja/?keyword=&status%5B%5D=prodam&type%5B%5D=2-5-sobno&type%5B%5D=3-sobno&states%5B%5D=podravska&location%5B%5D=&areas%5B%5D=borova-vas&areas%5B%5D=bresternica&areas%5B%5D=brezje&areas%5B%5D=center&areas%5B%5D=grusova&areas%5B%5D=kamnica&areas%5B%5D=koroska-vrata&areas%5B%5D=kosaki&areas%5B%5D=laznica&areas%5B%5D=limbus&areas%5B%5D=magdalena&areas%5B%5D=malecnik&areas%5B%5D=maribor&areas%5B%5D=melje&areas%5B%5D=miklavz-na-dravskem-polju&areas%5B%5D=nova-vas&areas%5B%5D=pekel&areas%5B%5D=pesnica-pri-mariboru&areas%5B%5D=pobrezje&areas%5B%5D=pocehova&areas%5B%5D=podrocje&areas%5B%5D=razvanje&areas%5B%5D=rospoh-del&areas%5B%5D=slivnica-pri-mariboru&areas%5B%5D=spodnje-radvanje&areas%5B%5D=srednje&areas%5B%5D=studenci&areas%5B%5D=tabor&areas%5B%5D=tezno&areas%5B%5D=zgornje-radvanje&zamujeno=&min-price=120000&max-price=190000";
            using var httpClient = ClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var htmlContent = await response.Content.ReadAsStringAsync();
                return ParseData(htmlContent);
            }
            else
            {
                throw new InvalidOperationException($"Bolha provider api call failed Additional data {response.StatusCode}...");
            }
        }

        public override async Task<IEnumerable<FlatEntity>> GetFlatsDemo()
        {
            var htmlContent = await File.ReadAllTextAsync("test_files/galea_response.html");
            return ParseData(htmlContent);
        }

        private IEnumerable<FlatEntity> ParseData(string htmlContent)
        {
            var results = new List<FlatEntity>();

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'listing-view')]//div[contains(@class, 'item-listing-wrap')]");

            if (nodes == null)
                return results;

            foreach (var node in nodes)
            {
                var linkNode = node.SelectSingleNode(".//div[contains(@class, 'listing-thumb')]/a");
                var link = linkNode?.Attributes["href"]?.Value;

                var titleNode = node?.SelectSingleNode(".//h2");
                var title = titleNode?.InnerText.Trim();

                var priceNode = node?.SelectSingleNode(".//li[@class='item-price']");
                var price = priceNode?.InnerText.Trim();

                var sizeNode = node?.SelectSingleNode(".//li[@class='h-povrc5a1ina-bruto']/span[2]");
                var size = sizeNode?.InnerText.Trim();

                results.Add(new FlatEntity
                {
                    Name = title ?? string.Empty,
                    Price = price ?? string.Empty,
                    Size = size ?? string.Empty,
                    Link = link ?? string.Empty,
                    Provider = "Galea"
                });
            }

            return results;
        }
    }
}
