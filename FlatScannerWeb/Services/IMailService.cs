namespace FlatScannerWeb.Services;

public interface IMailService
{
    Task SendEmail(IEnumerable<string> recipients, string subject, string messageBody);
}
