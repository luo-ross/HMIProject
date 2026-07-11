using System.Security.Cryptography;

namespace RS.SetupApp.Core;

public sealed class RsaPssUpdateSignatureVerifier : IUpdateSignatureVerifier
{
    public bool Verify(string contentPath, string signaturePath, string trustedPublicKeyPath)
    {
        try
        {
            if (!File.Exists(contentPath) || !File.Exists(signaturePath) || !File.Exists(trustedPublicKeyPath))
            {
                return false;
            }

            byte[] content = File.ReadAllBytes(contentPath);
            byte[] signature = File.ReadAllBytes(signaturePath);
            string publicKeyPem = File.ReadAllText(trustedPublicKeyPath);
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            return rsa.VerifyData(content, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
