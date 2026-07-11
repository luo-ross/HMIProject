namespace RS.SetupApp.Core;

/// <summary>Restricts update sources to local files or HTTPS endpoints.</summary>
public static class RemoteSourcePolicy
{
    public static void EnsureAllowed(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new InvalidOperationException("An update source is required.");
        }

        if (!Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) || IsWindowsDrivePath(source))
        {
            return;
        }

        EnsureAllowed(uri);
    }

    public static void EnsureAllowed(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (uri.IsFile || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        throw new InvalidOperationException($"Update source '{uri}' must use HTTPS or a local file path.");
    }

    public static bool IsAllowed(string source)
    {
        try
        {
            EnsureAllowed(source);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool IsWindowsDrivePath(string source)
        => source.Length >= 3 && char.IsLetter(source[0]) && source[1] == ':' &&
           (source[2] == Path.DirectorySeparatorChar || source[2] == Path.AltDirectorySeparatorChar);
}
