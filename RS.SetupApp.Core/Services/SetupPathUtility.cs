namespace RS.SetupApp.Core;

public static class SetupPathUtility
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static string ResolveManifestRelativePath(string baseManifestPath, string? relativeOrAbsolutePath)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
        {
            return string.Empty;
        }

        if (Path.IsPathRooted(relativeOrAbsolutePath))
        {
            return relativeOrAbsolutePath;
        }

        string baseDirectory = Path.GetDirectoryName(baseManifestPath) ?? AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDirectory, relativeOrAbsolutePath));
    }

    public static string Quote(string value)
    {
        return $"\"{value}\"";
    }

    public static string ApplyTokens(string template, ProductManifest product)
    {
        return template
            .Replace("{Publisher}", SanitizePathSegment(product.Publisher), StringComparison.OrdinalIgnoreCase)
            .Replace("{ProductId}", SanitizePathSegment(product.ProductId), StringComparison.OrdinalIgnoreCase)
            .Replace("{DisplayName}", SanitizePathSegment(product.DisplayName), StringComparison.OrdinalIgnoreCase);
    }

    public static string SanitizePathSegment(string value)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        char[] buffer = value.Select(character => invalidChars.Contains(character) ? '_' : character).ToArray();
        return new string(buffer).Trim();
    }

    public static int CompareVersions(string left, string right)
    {
        if (Version.TryParse(left, out Version? leftVersion) && Version.TryParse(right, out Version? rightVersion))
        {
            return leftVersion.CompareTo(rightVersion);
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public static string ExpandCommandTemplate(string? commandTemplate, string executablePath)
    {
        string template = string.IsNullOrWhiteSpace(commandTemplate)
            ? "{exe} {file}"
            : commandTemplate!;

        return template
            .Replace("{exe}", Quote(executablePath), StringComparison.OrdinalIgnoreCase)
            .Replace("{file}", "\"%1\"", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsParentTraversal(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        string normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalized
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(segment => string.Equals(segment, "..", StringComparison.Ordinal));
    }

    public static bool IsPathUnderRoot(string path, string rootPath)
    {
        string candidate = Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string root = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (candidate.Length == root.Length)
        {
            return PathComparer.Equals(candidate, root);
        }

        return candidate.StartsWith(
            $"{root}{Path.DirectorySeparatorChar}",
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    public static string ExpandEnvironmentTokens(string template)
    {
        return template
            .Replace("{ProgramFiles}", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), StringComparison.OrdinalIgnoreCase)
            .Replace("{LocalAppData}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase)
            .Replace("{CommonAppData}", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), StringComparison.OrdinalIgnoreCase)
            .Replace("{Temp}", Path.GetTempPath(), StringComparison.OrdinalIgnoreCase);
    }

    public static string? TryExtractExecutablePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        ReadOnlySpan<char> span = command.AsSpan().Trim();
        if (span.IsEmpty)
        {
            return null;
        }

        if (span[0] == '"')
        {
            int endIndex = span[1..].IndexOf('"');
            return endIndex >= 0 ? span.Slice(1, endIndex).ToString() : null;
        }

        int separatorIndex = span.IndexOf(' ');
        return separatorIndex < 0 ? span.ToString() : span[..separatorIndex].ToString();
    }
}
