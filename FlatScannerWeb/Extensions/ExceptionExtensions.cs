namespace FlatScannerWeb.Extensions;

public static class ExceptionExtensions
{
    public static string ToMailFormat(this Exception ex)
    {
        string formattedDetails = $"----------------------------------------------------------------------<br />";
        formattedDetails += $"Type: {ex.GetType().FullName}<br />";
        formattedDetails += $"Message: {ex.Message}<br />";

        // Format stack trace
        var stackTraceLines = ex.StackTrace?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        formattedDetails += $"StackTrace:\n{string.Join("<br />", stackTraceLines ?? Array.Empty<string>())}<br />";

        return formattedDetails;
    }
}
