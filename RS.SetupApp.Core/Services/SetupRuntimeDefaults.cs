namespace RS.SetupApp.Core;

public static class SetupRuntimeDefaults
{
    public const string ProductManifestFileName = "product.json";
    public const string ProductSchemaFileName = "product.schema.json";
    public const string PackageManifestFileName = "package.manifest.json";
    public const string PackageManifestSignatureFileName = "package.manifest.json.sig";
    public const string UpdateManifestFileName = "latest.json";
    public const string UpdateManifestSignatureFileName = "latest.json.sig";
    public const string TrustedPublicKeyFileName = "update.public.pem";
    public const string DefaultPayloadFolderName = "payload";
    public const string MaintenanceFolderName = "InstallerBundle";
    public const string OwnershipMarkerFileName = ".rs-setup-owner.json";
}
