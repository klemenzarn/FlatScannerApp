namespace FlatScannerWeb.Entities
{
    public class AppSettings
    {
        public bool UseDemoData { get; set; }

        public MailConfiguration Mail { get; set; } = new();
        public Providers Providers { get; set; } = new();
    }

    public class MailConfiguration
    {
        public SmtpSettings Smtp { get; set; } = new();
        public List<string> NotificationRecipients { get; set; } = new();
        public List<string> DevTeamRecipients { get; set; } = new();

    }

    public class SmtpSettings
    {
        public bool Enabled { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Providers
    {
        public ProviderData Bolha { get; set; } = new();
        public ProviderData Nepremicnine { get; set; } = new();
        public ProviderData Century21 { get; set; } = new();
        public ProviderData DoDoma { get; set; } = new();
        public ProviderData Nep24 { get; set; } = new();
        public ProviderData Galea { get; set; } = new();
    }

    public class ProviderData
    {
        public bool Enabled { get; set; }
    }
}
