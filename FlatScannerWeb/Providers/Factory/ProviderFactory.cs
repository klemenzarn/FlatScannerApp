namespace FlatScannerWeb.Providers.Factory;

public class ProviderFactory : IProviderFactory
{
    private readonly IEnumerable<IFlatProvider> _providers;

    public ProviderFactory(IEnumerable<IFlatProvider> providers)
    {
        _providers = providers;
    }

    public IEnumerable<IFlatProvider> CreateProviders()
    {
        return _providers.Where(p => p.Enabled);
    }
}
