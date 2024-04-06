using FlatScannerWeb.Entities;

namespace FlatScannerWeb.Providers
{
    public abstract class FlatProviderBase
    {
        protected FlatProviderBase(IHttpClientFactory clientFactory, AppSettings appSettings)
        {
            ClientFactory = clientFactory;
            AppSettings = appSettings;
        }

        protected IHttpClientFactory ClientFactory { get; }
        protected AppSettings AppSettings { get; }

        public virtual async Task<IEnumerable<FlatEntity>> GetFlats()
        {
            return AppSettings.UseDemoData
                ? await GetFlatsDemo()
                : await GetFlatsApi();
        }

        public abstract Task<IEnumerable<FlatEntity>> GetFlatsApi();

        public abstract Task<IEnumerable<FlatEntity>> GetFlatsDemo();
    }
}
