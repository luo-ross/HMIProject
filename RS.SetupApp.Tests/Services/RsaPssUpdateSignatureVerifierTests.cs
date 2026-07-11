using System.Security.Cryptography;
using RS.SetupApp.Core;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class RsaPssUpdateSignatureVerifierTests
{
    [TestMethod]
    public void Verify_ShouldAcceptRawContentSignedWithTrustedKey()
    {
        using TempDirectoryScope temp = new();
        string content = Path.Combine(temp.DirectoryPath, "latest.json");
        string privateKey = Path.Combine(temp.DirectoryPath, "signing.private.pem");
        string publicKey = Path.Combine(temp.DirectoryPath, "trusted.public.pem");
        File.WriteAllBytes(content, "{\"version\":\"1.2.3\"}"u8.ToArray());
        using (RSA rsa = RSA.Create(2048))
        {
            File.WriteAllText(privateKey, rsa.ExportRSAPrivateKeyPem());
            File.WriteAllText(publicKey, rsa.ExportRSAPublicKeyPem());
        }

        RsaPssManifestSigner signer = new();
        string signature = signer.Sign(content, privateKey);

        Assert.IsTrue(new RsaPssUpdateSignatureVerifier().Verify(content, signature, publicKey));
    }

    [TestMethod]
    public void Verify_ShouldRejectMissingMalformedTamperedAndWrongKeySignatures()
    {
        using TempDirectoryScope temp = new();
        string content = Path.Combine(temp.DirectoryPath, "package.manifest.json");
        string privateKey = Path.Combine(temp.DirectoryPath, "signing.private.pem");
        string publicKey = Path.Combine(temp.DirectoryPath, "trusted.public.pem");
        string wrongPublicKey = Path.Combine(temp.DirectoryPath, "wrong.public.pem");
        File.WriteAllBytes(content, "{\"version\":\"1.2.3\"}"u8.ToArray());
        using (RSA rsa = RSA.Create(2048))
        {
            File.WriteAllText(privateKey, rsa.ExportRSAPrivateKeyPem());
            File.WriteAllText(publicKey, rsa.ExportRSAPublicKeyPem());
        }
        using (RSA wrong = RSA.Create(2048))
        {
            File.WriteAllText(wrongPublicKey, wrong.ExportRSAPublicKeyPem());
        }

        RsaPssManifestSigner signer = new();
        string signature = signer.Sign(content, privateKey);
        RsaPssUpdateSignatureVerifier verifier = new();

        Assert.IsFalse(verifier.Verify(Path.Combine(temp.DirectoryPath, "missing.json"), signature, publicKey));
        Assert.IsFalse(verifier.Verify(content, Path.Combine(temp.DirectoryPath, "missing.sig"), publicKey));
        File.WriteAllText(signature, "not-a-signature");
        Assert.IsFalse(verifier.Verify(content, signature, publicKey));
        signer.Sign(content, signature, privateKey);
        File.AppendAllText(content, " ");
        Assert.IsFalse(verifier.Verify(content, signature, publicKey));
        Assert.IsFalse(verifier.Verify(content, signature, wrongPublicKey));
        File.WriteAllText(publicKey, "not-a-public-key");
        Assert.IsFalse(verifier.Verify(content, signature, publicKey));
    }
}
