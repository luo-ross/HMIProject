using RS.SetupApp.Core;

namespace RS.SetupApp.Builder;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            BuilderOptions options = BuilderArgumentParser.Parse(args);
            JsonManifestSerializer serializer = new();
            DefaultFileHasher hasher = new();
            DotnetPublishRunner publishRunner = new();
            PackageBuilder packageBuilder = new(serializer, hasher, publishRunner);
            InstallerBundleBuilder installerBundleBuilder = new(serializer, publishRunner);
            UpdateFeedPublisher updateFeedPublisher = new(serializer);

            switch (options.Command)
            {
                case BuilderCommand.Validate:
                    return Validate(options, serializer);
                case BuilderCommand.Pack:
                    Console.WriteLine(await packageBuilder.BuildAsync(options, CancellationToken.None).ConfigureAwait(false));
                    return 0;
                case BuilderCommand.BuildInstaller:
                    Console.WriteLine(await installerBundleBuilder.BuildAsync(options, CancellationToken.None).ConfigureAwait(false));
                    return 0;
                case BuilderCommand.PublishUpdateFeed:
                    Console.WriteLine(updateFeedPublisher.Publish(options));
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int Validate(BuilderOptions options, JsonManifestSerializer serializer)
    {
        if (string.IsNullOrWhiteSpace(options.ProductManifestPath))
        {
            throw new ArgumentException("--product is required for validate.");
        }

        ProductManifestLoadResult loadResult = ProductManifestLoader.Load(Path.GetFullPath(options.ProductManifestPath), serializer);
        if (loadResult.Errors.Count == 0)
        {
            Console.WriteLine("Product manifest is valid.");
            return 0;
        }

        foreach (string error in loadResult.Errors)
        {
            Console.Error.WriteLine(error);
        }

        return 1;
    }
}
