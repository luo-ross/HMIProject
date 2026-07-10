using System.Text;
using RS.SetupApp.Builder;
using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Helpers;

public static class SetupTestDataFactory
{
    public static string WriteProductSchema(string directoryPath)
    {
        string schemaPath = Path.Combine(directoryPath, SetupRuntimeDefaults.ProductSchemaFileName);
        File.WriteAllText(schemaPath, """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "additionalProperties": false,
          "required": [ "productId", "displayName", "publisher", "mainExecutable" ],
          "properties": {
            "productId": { "type": "string", "minLength": 1 },
            "displayName": { "type": "string", "minLength": 1 },
            "publisher": { "type": "string", "minLength": 1 },
            "mainExecutable": { "type": "string", "minLength": 1 },
            "mainProcessName": { "type": "string" },
            "upgradeCode": { "type": "string" },
            "supportUrl": { "type": "string" },
            "updateInfoUrl": { "type": "string" },
            "branding": { "type": "object" },
            "installDefaults": { "type": "object" },
            "update": { "type": "object" },
            "uninstall": { "type": "object" },
            "shortcuts": { "type": "array" },
            "fileAssociations": { "type": "array" },
            "dataDirectories": { "type": "array" },
            "extensions": { "type": "array" }
          }
        }
        """, Encoding.UTF8);
        return schemaPath;
    }

    public static string WriteProductManifest(
        string directoryPath,
        string productId,
        string mainExecutable,
        string? manifestUrl = null,
        bool allowOverwrite = false)
    {
        JsonManifestSerializer serializer = new();
        string manifestPath = Path.Combine(directoryPath, SetupRuntimeDefaults.ProductManifestFileName);
        serializer.Save(manifestPath, new ProductManifest
        {
            ProductId = productId,
            DisplayName = "Demo App",
            Publisher = "Contoso",
            MainExecutable = mainExecutable,
            Branding = new BrandingManifest
            {
                IconPath = "assets/icon.ico",
                LicensePath = "LICENSE.txt",
                WelcomeText = "Generic installer test payload."
            },
            InstallDefaults = new InstallDefaultsManifest
            {
                DefaultScope = InstallScope.CurrentUser,
                AllowSilentInstall = true,
                AllowOverwrite = allowOverwrite,
                AllowRepair = true,
                AllowMachineInstall = true,
                MinimumFreeSpaceBytes = 0
            },
            Update = new UpdateSettingsManifest
            {
                AllowOnlineUpdate = !string.IsNullOrWhiteSpace(manifestUrl),
                ManifestUrl = manifestUrl,
                Channel = "stable"
            },
            Uninstall = new UninstallPolicyManifest
            {
                AllowPurgeData = true,
                PurgeDataByDefault = false
            },
            Shortcuts =
            [
                new ShortcutManifest
                {
                    Location = ShortcutLocation.Desktop,
                    EnabledByDefault = true
                }
            ],
            DataDirectories =
            [
                new DataDirectoryManifest
                {
                    Key = "userData",
                    Scope = DataDirectoryScope.UserLocal,
                    RelativePath = Path.Combine("Contoso", productId)
                }
            ]
        });

        string assetsDirectory = Path.Combine(directoryPath, "assets");
        Directory.CreateDirectory(assetsDirectory);
        File.WriteAllBytes(Path.Combine(assetsDirectory, "icon.ico"), [0x00]);
        File.WriteAllText(Path.Combine(directoryPath, "LICENSE.txt"), "Sample license.", Encoding.UTF8);
        return manifestPath;
    }

    public static string CreatePublishDirectory(string rootPath, string executableName, string version, params string[] extraFiles)
    {
        string publishDirectory = Path.Combine(rootPath, "publish", version);
        Directory.CreateDirectory(publishDirectory);
        File.WriteAllBytes(Path.Combine(publishDirectory, executableName), [0x4D, 0x5A, 0x90, 0x00]);
        foreach (string fileName in extraFiles)
        {
            string fullPath = Path.Combine(publishDirectory, fileName);
            string? parentDirectory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            File.WriteAllText(fullPath, $"payload for {fileName}", Encoding.UTF8);
        }

        return publishDirectory;
    }

    public static async Task<string> CreatePackageAsync(string publishDirectory, string productManifestPath, string outputDirectory, string? packageVersion = null)
    {
        JsonManifestSerializer serializer = new();
        PackageBuilder builder = new(serializer, new DefaultFileHasher(), new DotnetPublishRunner());
        string packageDirectory = await builder.BuildAsync(new BuilderOptions
        {
            Command = BuilderCommand.Pack,
            FromDirectory = publishDirectory,
            ProductManifestPath = productManifestPath,
            OutputDirectory = outputDirectory
        }, CancellationToken.None).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(packageVersion))
        {
            string manifestPath = Path.Combine(packageDirectory, SetupRuntimeDefaults.PackageManifestFileName);
            PackageManifest manifest = serializer.Load<PackageManifest>(manifestPath);
            manifest.Version = packageVersion;
            serializer.Save(manifestPath, manifest);
        }

        return packageDirectory;
    }
}
