using Microsoft.Win32;
using System.IO;

namespace RS.SetupApp.Services;

public interface IFolderPicker
{
    Task<string?> PickAsync(string? initialDirectory, string description, CancellationToken cancellationToken);
}

public sealed class FolderPicker : IFolderPicker
{
    public Task<string?> PickAsync(string? initialDirectory, string description, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        OpenFolderDialog dialog = new()
        {
            Title = description,
            Multiselect = false,
            InitialDirectory = !string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory)
                ? initialDirectory
                : null,
            FolderName = initialDirectory
        };
        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FolderName : null);
    }
}

public sealed class NoopFolderPicker : IFolderPicker
{
    public Task<string?> PickAsync(string? initialDirectory, string description, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
}
