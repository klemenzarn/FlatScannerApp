namespace FlatScannerWeb.Entities
{
    public class MailRecipientsOptions
    {
        public List<string> NotificationRecipients { get; set; } = new();
        public List<string> DevTeamRecipients { get; set; } = new();

    }
}
