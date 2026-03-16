using System.Security.Cryptography;

namespace RS.SetupApp.Core;

public sealed class DefaultFileHasher : IFileHasher
{
    public string ComputeSha256(string path)
    {
        using SHA256 sha256 = SHA256.Create();
        using FileStream stream = File.OpenRead(path);
        byte[] hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
