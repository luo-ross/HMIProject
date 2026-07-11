using RS.SetupApp.Core;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class RemoteSourcePolicyTests
{
    [DataTestMethod]
    [DataRow("http://example.test/latest.json")]
    [DataRow("ftp://example.test/latest.json")]
    public void EnsureAllowed_ShouldRejectInsecureRemoteSources(string source)
    {
        Assert.ThrowsException<InvalidOperationException>(() => RemoteSourcePolicy.EnsureAllowed(source));
    }

    [DataTestMethod]
    [DataRow("https://example.test/latest.json")]
    [DataRow("C:\\updates\\latest.json")]
    [DataRow("file:///C:/updates/latest.json")]
    public void EnsureAllowed_ShouldAcceptHttpsAndLocalSources(string source)
    {
        RemoteSourcePolicy.EnsureAllowed(source);
    }
}
