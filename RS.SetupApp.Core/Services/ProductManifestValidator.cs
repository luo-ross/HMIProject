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

        if (manifest.Update.AllowOnlineUpdate && string.IsNullOrWhiteSpace(manifest.Update.ManifestUrl))
        {
            errors.Add("update.manifestUrl is required when allowOnlineUpdate is true.");
        }

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
}
