using System.Text.Json.Serialization;

namespace RS.SetupApp.Core;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataDirectoryScope
{
    UserLocal,
    CommonAppData
}
