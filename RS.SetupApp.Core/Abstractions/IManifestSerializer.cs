namespace RS.SetupApp.Core;

public interface IManifestSerializer
{
    T Load<T>(string path);

    void Save<T>(string path, T value);

    string Serialize<T>(T value);
}
