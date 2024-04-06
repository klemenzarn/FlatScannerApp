using FlatScannerWeb.Entities;
using System.Collections.Concurrent;

namespace FlatScannerWeb.DataAccess;

public class InMemoryDatabase : IDatabase
{
    private ConcurrentDictionary<string, FlatEntity> _flatsDictionary = new();

    public Task<IEnumerable<FlatEntity>> GetFlats()
    {
        var result = _flatsDictionary.Values;

        return Task.FromResult(result.AsEnumerable());
    }

    public Task SaveFlats(IEnumerable<FlatEntity> flats)
    {
        foreach (var flat in flats)
        {
            _flatsDictionary.TryAdd(flat.Id, flat);
        }

        return Task.CompletedTask;
    }
}
