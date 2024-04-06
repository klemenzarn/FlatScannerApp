using FlatScannerWeb.Entities;

namespace FlatScannerWeb.DataAccess;

public interface IDatabase
{
    Task<IEnumerable<FlatEntity>> GetFlats();

    Task SaveFlats(IEnumerable<FlatEntity> flats);
}
