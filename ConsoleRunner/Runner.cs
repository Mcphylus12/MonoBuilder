using MonoBuilder;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleRunner;
internal class Runner
{
    public static void Run(string? fileIn, string? configFile)
    {
        Config? config = null;

        try
        {
            config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile), SourceGenerationContext.Default.Config);
        }
        catch
        {
            Console.Error.WriteLine("Error loading config");
        }

        var changes = LoadChanges(fileIn);

        var execs = GetExecs(config!, changes);

        Console.WriteLine(string.Join(Environment.NewLine, execs));
    }

    static string[] LoadChanges(string? fileIn)
    {
        if (!string.IsNullOrEmpty(fileIn))
        {
            return File.ReadAllLines(fileIn);
        }
        else
        {
            return Console.In.ReadToEnd().Split(Environment.NewLine);
        }
    }

    static IEnumerable<string> GetExecs(Config? config, string[] changes)
    {
        var projects = new ChangeResolver(config)
            .GetChangedProjects(changes.Select(s => s.Trim()));

        return projects.Select(p => p.Exec).Where(exec => !string.IsNullOrEmpty(exec))!;
    }

}

[JsonSerializable(typeof(Config))]
public partial class SourceGenerationContext : JsonSerializerContext
{
}