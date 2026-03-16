namespace RS.SetupApp.Builder;

public static class BuilderArgumentParser
{
    public static BuilderOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            throw new ArgumentException("A builder command is required.");
        }

        BuilderOptions options = new()
        {
            Command = ParseCommand(args[0])
        };

        for (int index = 1; index < args.Count; index++)
        {
            string current = args[index];
            switch (current)
            {
                case "--from-dir":
                    options.FromDirectory = GetNextValue(args, ref index, "--from-dir");
                    break;
                case "--from-project":
                    options.FromProject = GetNextValue(args, ref index, "--from-project");
                    break;
                case "--product":
                    options.ProductManifestPath = GetNextValue(args, ref index, "--product");
                    break;
                case "--package":
                    options.PackageDirectory = GetNextValue(args, ref index, "--package");
                    break;
                case "--output":
                    options.OutputDirectory = GetNextValue(args, ref index, "--output");
                    break;
                case "--configuration":
                    options.Configuration = GetNextValue(args, ref index, "--configuration");
                    break;
                case "--runtime":
                    options.Runtime = GetNextValue(args, ref index, "--runtime");
                    break;
                case "--channel":
                    options.Channel = GetNextValue(args, ref index, "--channel");
                    break;
                case "--base-url":
                    options.BaseUrl = GetNextValue(args, ref index, "--base-url");
                    break;
                case "--runtime-project":
                    options.RuntimeProjectPath = GetNextValue(args, ref index, "--runtime-project");
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{current}'.");
            }
        }

        return options;
    }

    private static BuilderCommand ParseCommand(string command)
    {
        return command.ToLowerInvariant() switch
        {
            "validate" => BuilderCommand.Validate,
            "pack" => BuilderCommand.Pack,
            "build-installer" => BuilderCommand.BuildInstaller,
            "publish-update-feed" => BuilderCommand.PublishUpdateFeed,
            _ => throw new ArgumentException($"Unsupported builder command '{command}'.")
        };
    }

    private static string GetNextValue(IReadOnlyList<string> args, ref int index, string argumentName)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value for '{argumentName}'.");
        }

        index++;
        return args[index];
    }
}
