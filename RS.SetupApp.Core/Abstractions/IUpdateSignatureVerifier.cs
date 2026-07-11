namespace RS.SetupApp.Core;

public interface IUpdateSignatureVerifier
{
    bool Verify(string contentPath, string signaturePath, string trustedPublicKeyPath);
}
