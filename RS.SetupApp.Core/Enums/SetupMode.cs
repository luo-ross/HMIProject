using System.Text.Json.Serialization;

namespace RS.SetupApp.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SetupMode
{
    Install,
    Repair,
    Update,
    Uninstall,
    ApplyPackage,
    SelfUpdateWorker
}
