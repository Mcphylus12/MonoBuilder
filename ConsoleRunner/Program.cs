using ConsoleRunner;
using System.CommandLine;

var rootCommand = new RootCommand("Sample CLI app");

var fileInOption = new Option<string>(
    name: "--fileIn",
    description: "Use File input for changed files instead of standard in"
);
rootCommand.AddOption(fileInOption);

var configOption = new Option<string>(
    name: "--config",
    description: "Project Config"
);
rootCommand.AddOption(configOption);

rootCommand.SetHandler(Runner.Run, fileInOption, configOption);

return await rootCommand.InvokeAsync(args);