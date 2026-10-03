using System.Globalization;

namespace Bacon.ApiUtilities.Services;

internal static class MessageFormatterService
{
    public static string FormatErrorResultMessage(string message, IEnumerable<object>? metadata)
    {
        if (metadata is null)
        {
            return message;
        }

        try
        {
            return string.Format(CultureInfo.InvariantCulture, message, metadata.ToArray());
        }
        catch (FormatException)
        {
            return message;
        }
    }
}