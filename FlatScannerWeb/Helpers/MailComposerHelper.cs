using FlatScannerWeb.Entities;
using FlatScannerWeb.Extensions;
using System.Collections.Concurrent;
using System.Text;

namespace FlatScannerWeb.Helpers;

public static class MailComposerHelper
{
    public static string CreateNewFlatsMailBody(IEnumerable<FlatEntity> newFlats)
    {
        var bodyBuilder = new StringBuilder();
        bodyBuilder.Append("Objavljena so nova stanovanja!<br />");
        bodyBuilder.Append("Kako se ti zdijo?<br /><br />");

        foreach (var flatGroup in newFlats.GroupBy(p => p.Provider))
        {
            bodyBuilder.Append($"<b>{flatGroup.Key}</b>");
            bodyBuilder.Append("<ul>");

            foreach (var flat in flatGroup)
                bodyBuilder.Append($"<li><a href='{flat.Link}'>{flat.Name}</a> (velikost: {flat.Size}, cena: {flat.Price})</li>");

            bodyBuilder.Append("</ul>");
        }

        return bodyBuilder.ToString();
    }

    public static string CreateExceptionsMailBody(ConcurrentBag<Exception> exceptions)
    {
        var bodyBuilder = new StringBuilder();

        bodyBuilder.Append("Zgodile so se nekatere napake pri branju podatkov:<br />");

        foreach (var ex in exceptions)
        {
            bodyBuilder.Append(ex.ToMailFormat());
        }

        return bodyBuilder.ToString();
    }
}
