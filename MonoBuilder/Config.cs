using System.Text.Json.Serialization;

namespace MonoBuilder;

public class Config
{
    public required List<Project> Projects { get; set; }
}

public class Project
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public string? Exec { get; set; }
    public IList<string> Dependencies { get; set; } = [];
}
