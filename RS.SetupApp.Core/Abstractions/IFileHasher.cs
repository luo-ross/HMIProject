namespace RS.SetupApp.Core;

public interface IFileHasher
{
    string ComputeSha256(string path);
}
