namespace RS.SetupApp.Core;

public static class ProductManifestValidator
{
    public static IReadOnlyList<string> Validate(ProductManifest manifest, string? productManifestPath = null, Func<string, bool>? fileExists = null)
    {
        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(manifest.ProductId))
        {
            errors.Add("productId is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.DisplayName))
        {
            errors.Add("displayName is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Publisher))
        {
            errors.Add("publisher is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.MainExecutable))
        {
            errors.Add("mainExecutable is required.");
        }
        else if (Path.IsPathRooted(manifest.MainExecutable))
        {
            errors.Add("mainExecutable must be relative to the installed application directory.");
        }

        if (manifest.FileAssociations.GroupBy(item => item.Extension, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            errors.Add("fileAssociations contains duplicate extensions.");
        }

        if (manifest.DataDirectories.GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            errors.Add("dataDirectories contains duplicate keys.");
        }

        if (!manifest.InstallDefaults.AllowMachineInstall && manifest.InstallDefaults.DefaultScope == InstallScope.AllUsers)
        {
            errors.Add("installDefaults.defaultScope cannot be AllUsers when allowMachineInstall is false.");
        }

        foreach (FileAssociationManifest association in manifest.FileAssociations)
        {
            if (string.IsNullOrWhiteSpace(association.Extension) || !association.Extension.StartsWith(".", StringComparison.Ordinal))
            {
                errors.Add("Each file association extension must start with '.'.");
            }

            if (string.IsNullOrWhiteSpace(association.ProgId))
            {
                errors.Add($"File association '{association.Extension}' must define progId.");
            }

            if (!string.IsNullOrWhiteSpace(association.IconPath) &&
                SetupPathUtility.ContainsParentTraversal(association.IconPath))
            {
                errors.Add($"File association '{association.Extension}' iconPath cannot contain '..'.");
            }
        }

        foreach (DataDirectoryManifest dataDirectory in manifest.DataDirectories)
        {
            if (string.IsNullOrWhiteSpace(dataDirectory.Key))
            {
                errors.Add("Each dataDirectories item must define key.");
            }

            if (string.IsNullOrWhiteSpace(dataDirectory.RelativePath))
            {
                errors.Add($"dataDirectories '{dataDirectory.Key}' must define relativePath.");
            }
            else if (Path.IsPathRooted(dataDirectory.RelativePath) || SetupPathUtility.ContainsParentTraversal(dataDirectory.RelativePath))
            {
                errors.Add($"dataDirectories '{dataDirectory.Key}' relativePath must stay within the configured data root.");
            }
        }

        ValidateOnlineUpdateSettings(errors, manifest, productManifestPath, fileExists);

        if (!string.IsNullOrWhiteSpace(manifest.InstallDefaults.DefaultInstallDirectoryOverride) &&
            SetupPathUtility.ContainsParentTraversal(manifest.InstallDefaults.DefaultInstallDirectoryOverride))
        {
            errors.Add("installDefaults.defaultInstallDirectoryOverride cannot contain '..'.");
        }

        foreach (ExtensionRegistrationManifest extension in manifest.Extensions)
        {
            if (string.IsNullOrWhiteSpace(extension.AssemblyPath))
            {
                errors.Add("Each extensions item must define assemblyPath.");
            }

            if (string.IsNullOrWhiteSpace(extension.TypeName))
            {
                errors.Add("Each extensions item must define typeName.");
            }
        }

        if (!string.IsNullOrWhiteSpace(productManifestPath) && fileExists != null)
        {
            ValidateReferencedFile(errors, productManifestPath, manifest.Branding.IconPath, "branding.iconPath", fileExists);
            ValidateReferencedFile(errors, productManifestPath, manifest.Branding.LicensePath, "branding.licensePath", fileExists);

            foreach (ShortcutManifest shortcut in manifest.Shortcuts)
            {
                ValidateReferencedFile(errors, productManifestPath, shortcut.IconPath, $"shortcut '{shortcut.Name}' iconPath", fileExists);
            }

            foreach (FileAssociationManifest association in manifest.FileAssociations)
            {
                ValidateReferencedFile(errors, productManifestPath, association.IconPath, $"fileAssociation '{association.Extension}' iconPath", fileExists);
            }

            foreach (ExtensionRegistrationManifest extension in manifest.Extensions.Where(item => !item.Optional))
            {
                ValidateReferencedFile(errors, productManifestPath, extension.AssemblyPath, $"extension '{extension.TypeName}' assemblyPath", fileExists);
            }
        }

        return errors;
    }

    private static void ValidateReferencedFile(
        ICollection<string> errors,
        string productManifestPath,
        string? relativePath,
        string label,
        Func<string, bool> fileExists)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        if (SetupPathUtility.ContainsParentTraversal(relativePath))
        {
            errors.Add($"{label} cannot contain '..'.");
            return;
        }

        string resolvedPath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, relativePath);
        if (!fileExists(resolvedPath))
        {
            errors.Add($"{label} file '{resolvedPath}' was not found.");
        }
    }

    private static void ValidateOnlineUpdateSettings(
        ICollection<string> errors,
        ProductManifest manifest,
        string? productManifestPath,
        Func<string, bool>? fileExists)
    {
        if (!manifest.Update.AllowOnlineUpdate)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(manifest.Update.ManifestUrl))
        {
            errors.Add("update.manifestUrl is required when allowOnlineUpdate is true.");
        }
        else if (!Uri.TryCreate(manifest.Update.ManifestUrl, UriKind.Absolute, out Uri? manifestUri) ||
                 !string.Equals(manifestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("update.manifestUrl must use HTTPS when allowOnlineUpdate is true.");
        }

        if (!manifest.Update.RequireHttps)
        {
            errors.Add("update.requireHttps must be true when allowOnlineUpdate is true.");
        }

        if (!manifest.Update.RequireSignature)
        {
            errors.Add("update.requireSignature must be true when allowOnlineUpdate is true.");
        }

        string? publicKeyPath = manifest.Update.TrustedPublicKeyPath;
        if (string.IsNullOrWhiteSpace(publicKeyPath))
        {
            errors.Add("update.trustedPublicKeyPath is required when allowOnlineUpdate is true.");
            return;
        }

        if (Path.IsPathRooted(publicKeyPath) || SetupPathUtility.ContainsParentTraversal(publicKeyPath))
        {
            errors.Add("update.trustedPublicKeyPath must be relative to the product manifest directory and cannot contain '..'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(productManifestPath))
        {
            return;
        }

        string productDirectory = Path.GetDirectoryName(Path.GetFullPath(productManifestPath)) ?? AppContext.BaseDirectory;
        string resolvedPublicKeyPath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, publicKeyPath);
        if (!SetupPathUtility.IsPathUnderRoot(resolvedPublicKeyPath, productDirectory))
        {
            errors.Add("update.trustedPublicKeyPath must stay under the product manifest directory.");
        }
        else if (fileExists != null && !fileExists(resolvedPublicKeyPath))
        {
            errors.Add($"update.trustedPublicKeyPath file '{resolvedPublicKeyPath}' was not found.");
        }
    }
}
