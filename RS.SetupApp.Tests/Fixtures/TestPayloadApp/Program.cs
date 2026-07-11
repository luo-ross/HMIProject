using System.Reflection;

string versionFile = Path.Combine(AppContext.BaseDirectory, "fixture-version.txt");
string version = File.Exists(versionFile)
    ? File.ReadAllText(versionFile).Trim()
    : Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

Console.WriteLine($"RS.SetupApp disposable test payload {version}");
