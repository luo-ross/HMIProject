using System.Security.Cryptography;

namespace RS.SetupApp.Core;

public sealed class RsaPssManifestSigner
{
    public string Sign(string contentPath, string privateKeyPath)
    {
        string signaturePath = $"{contentPath}.sig";
        Sign(contentPath, signaturePath, privateKeyPath);
        return signaturePath;
    }

    public void Sign(string contentPath, string signaturePath, string privateKeyPath)
    {
        byte[] content = File.ReadAllBytes(contentPath);
        string privateKeyPem = File.ReadAllText(privateKeyPath);
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        byte[] signature = rsa.SignData(content, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        File.WriteAllBytes(signaturePath, signature);
    }

    public void ExportPublicKey(string privateKeyPath, string publicKeyPath)
    {
        string privateKeyPem = File.ReadAllText(privateKeyPath);
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        string? directory = Path.GetDirectoryName(publicKeyPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(publicKeyPath, rsa.ExportRSAPublicKeyPem());
    }
}
