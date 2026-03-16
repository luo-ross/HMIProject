using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.CommandLine;

[TestClass]
public sealed class RuntimeArgumentParserTests
{
    [TestMethod]
    public void Parse_ShouldReadAllSupportedArguments()
    {
        RuntimeOptions options = RuntimeArgumentParser.Parse(
        [
            "--mode", "update",
            "--scope", "machine",
            "--product", "product.json",
            "--package", "package.zip",
            "--manifest", "package.manifest.json",
            "--install-dir", "C:\\Apps\\Demo",
            "--no-shortcuts",
            "--no-autostart",
            "--purge-data",
            "--worker",
            "--elevated",
            "--launch",
            "--skip-launch"
        ]);

        Assert.AreEqual(SetupMode.Update, options.Mode);
        Assert.AreEqual(InstallScope.AllUsers, options.Scope);
        Assert.AreEqual("product.json", options.ProductManifestPath);
        Assert.AreEqual("package.zip", options.PackagePath);
        Assert.AreEqual("package.manifest.json", options.PackageManifestPath);
        Assert.AreEqual("C:\\Apps\\Demo", options.InstallDirectory);
        Assert.IsTrue(options.NoShortcuts);
        Assert.IsTrue(options.NoAutostart);
        Assert.IsTrue(options.PurgeData);
        Assert.IsTrue(options.Worker);
        Assert.IsTrue(options.Elevated);
        Assert.IsTrue(options.LaunchAfterInstall);
        Assert.IsTrue(options.SkipLaunch);
    }
}
