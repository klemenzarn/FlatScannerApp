namespace FlatScannerWeb.Providers.Factory;

public interface IProviderFactory
{
    IEnumerable<IFlatProvider> CreateProviders();
}
