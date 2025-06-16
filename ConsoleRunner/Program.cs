using ConsoleRunner;
using System.CommandLine;

var rootCommand = new RootCommand(
"""
Monobuilder takes in a set of changed files and folders as well as project config that maps project dependencies and build scripts.
When passed the 2 inputs it prints to stdout the build scripts for all projects that have one configured and need building.

Location of the config file is required to be passed.
By default changes will be pulled from standard in but a flags can be set to pull them from a file if desired
"""
    );

var fileInOption = new Option<string>(
    name: "--fileIn",
    description: "Use File input for changed files instead of standard in"
);
fileInOption.IsRequired = false;
rootCommand.AddOption(fileInOption);

var configOption = new Option<string>(
    name: "--config",
    description: "Project Config"
);
configOption.IsRequired = true;
rootCommand.AddOption(configOption);

rootCommand.SetHandler(Runner.Run, fileInOption, configOption);

return await rootCommand.InvokeAsync(args);