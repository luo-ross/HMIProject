using RS.SetupApp.Builder;

namespace RS.SetupApp.Tests.CommandLine;

[TestClass]
public sealed class BuilderArgumentParserTests
{
    [TestMethod]
    public void Parse_ShouldReadPackArguments()
    {
        BuilderOptions options = BuilderArgumentParser.Parse(
        [
            "pack",
            "--from-dir", "publish",
            "--product", "product.json",
            "--output", "artifacts",
            "--runtime", "win-x64"
        ]);

        Assert.AreEqual(BuilderCommand.Pack, options.Command);
        Assert.AreEqual("publish", options.FromDirectory);
        Assert.AreEqual("product.json", options.ProductManifestPath);
        Assert.AreEqual("artifacts", options.OutputDirectory);
        Assert.AreEqual("win-x64", options.Runtime);
    }

    [TestMethod]
    public void Parse_ShouldReadSigningKeyWithoutExposingIt()
    {
        BuilderOptions options = BuilderArgumentParser.Parse(
        [
            "publish-update-feed",
            "--package", "artifacts",
            "--signing-key", "keys/release.private.pem"
        ]);

        Assert.AreEqual("keys/release.private.pem", options.SigningKeyPath);
    }
}
