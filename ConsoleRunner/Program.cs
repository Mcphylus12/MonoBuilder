using MonoBuilder;
using System.CommandLine;
using System.Text.Json;

var rootCommand = new RootCommand("Sample CLI app");

// Add options
var standardInOption = new Option<bool?>(
    name: "--stdin",
    description: "Use Standard input for changed files"
);
rootCommand.AddOption(standardInOption);

var fileInOption = new Option<string>(
    name: "--fileIn",
    description: "Use File input for changed files"
);
rootCommand.AddOption(fileInOption);

var configOption = new Option<string>(
    name: "--config",
    description: "Project Config"
);
rootCommand.AddOption(configOption);

// Set the handler
rootCommand.SetHandler((bool? readFromStandardIn, string? fileIn, string? configFile) =>
{
    if (readFromStandardIn.GetValueOrDefault() && !string.IsNullOrEmpty(fileIn))
    {
        throw new NotSupportedException("Cant pull input from std in and a file at same time");
    }

    if (!readFromStandardIn.GetValueOrDefault() && string.IsNullOrEmpty(fileIn))
    {
        throw new NotSupportedException("need to pass a flag to say where input is from");
    }

    var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile));

    if (!string.IsNullOrEmpty(fileIn))
    {
        var changes = File.ReadAllLines(fileIn);

        var projects = new ChangeResolver(config).GetChangedProjects(changes);

        Console.WriteLine(string.Join(Environment.NewLine, projects.Select(p => p.Name)));
    }
    else
    {
        var changes = Console.In.ReadToEnd().Split(Environment.NewLine);

        var projects = new ChangeResolver(config).GetChangedProjects(changes);

        Console.WriteLine(string.Join(Environment.NewLine, projects.Select(p => p.Name)));
    }
},
standardInOption, fileInOption, configOption);

return await rootCommand.InvokeAsync(args);
