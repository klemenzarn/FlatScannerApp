namespace FlatScannerWeb.Entities
{
    public class MailSmtpOptions
    {
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
