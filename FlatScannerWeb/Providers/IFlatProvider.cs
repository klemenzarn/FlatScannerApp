using FlatScannerWeb.Entities;

namespace FlatScannerWeb.Providers;

public interface IFlatProvider
{
    bool Enabled { get; }

    Task<IEnumerable<FlatEntity>> GetFlats();
}