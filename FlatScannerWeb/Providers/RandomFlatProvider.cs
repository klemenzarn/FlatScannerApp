using FlatScannerWeb.Entities;

namespace FlatScannerWeb.Providers;

public class RandomFlatProvider : IFlatProvider
{
    public bool Enabled => false;

    Random random = new Random();

    public Task<IEnumerable<FlatEntity>> GetFlats()
    {
        return Task.FromResult(GenerateRandomFlatEntities());
    }

    private IEnumerable<FlatEntity> GenerateRandomFlatEntities()
    {
        List<FlatEntity> entities = new List<FlatEntity>();

        int numberOfEntities = random.Next(1, 11); // Random number of entities between 1 and 10

        for (int i = 0; i < numberOfEntities; i++)
        {
            entities.Add(new FlatEntity
            {
                Name = "Stanovanje " + GetRandomString(),
                Address = GetRandomString(),
                Description = GetRandomString(),
                Image = GetRandomString(),
                Price = random.Next(100000, 150000).ToString() + " €",
                Size = GetRandomString(),
                Rooms = GetRandomString(),
                Floor = GetRandomString(),
                Year = GetRandomString(),
                Location = GetRandomString(),
                Link = GetRandomString()
            });
        }

        return entities;
    }

    private string GetRandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, random.Next(5, 20))
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string GetRandomNumber()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, random.Next(5, 20))
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
