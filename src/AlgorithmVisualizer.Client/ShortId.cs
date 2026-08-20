namespace AlgorithmVisualizer.Client;

public static class ShortId
{
    public static bool TryNormalize(string? rawId, out string normalizedId, out string error)
    {
        normalizedId = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(rawId))
        {
            error = "Enter the 6-character ID shown under an element.";
            return false;
        }

        var compact = rawId.Trim().TrimStart('#').Replace("-", string.Empty, StringComparison.Ordinal);
        if (compact.Length != 6)
        {
            error = "Use the 6-character ID shown in the app, for example #A31F9C.";
            return false;
        }

        if (compact.Any(character => !Uri.IsHexDigit(character)))
        {
            error = "An ID uses 6 characters from 0-9 and A-F.";
            return false;
        }

        normalizedId = compact.ToUpperInvariant();
        return true;
    }
}
