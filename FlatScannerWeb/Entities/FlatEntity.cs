using System.Security.Cryptography;
using System.Text;

namespace FlatScannerWeb.Entities;

public class FlatEntity
{
    public string Id => GenerateUniqueID();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Rooms { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"Id: {Id}, Name: {Name}, Provider: {Provider}, Address: {Address}, Description: {Description}, Image: {Image}, Price: {Price}, Size: {Size}, Rooms: {Rooms}, Floor: {Floor}, Year: {Year}, Location: {Location}, Link: {Link}";
    }

    private string GenerateUniqueID()
    {
        return GenerateUniqueID(new [] { Name, Address, Description, Image, Price, Size, Rooms, Floor, Year, Location, Link, Provider });
    }

    private string GenerateUniqueID(IEnumerable<string> strings)
    {
        var combinedString = string.Join("", strings);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}