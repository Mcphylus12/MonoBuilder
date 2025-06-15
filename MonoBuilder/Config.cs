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

    [JsonIgnore]
    public IList<string> Dependents { get; set; } = [];


}

public class ChangeResolver
{
    private readonly Dictionary<string, Project> config;
    private readonly Dictionary<string, IDiscoverer> discoverers;

    public ChangeResolver(Config config, Dictionary<string, IDiscoverer>? discoverers = null)
    {
        this.config = config.Projects.ToDictionary(p => p.Name);
        this.discoverers = discoverers ?? [];
        AddDependents();
    }

    private void AddDependents()
    {
        foreach (var project in config.Values)
        {
            foreach (var dep in project.Dependencies)
            {
                if (this.config.TryGetValue(dep, out var dependency))
                {
                    dependency.Dependents.Add(project.Name);
                } 
                else if (this.discoverers?.TryGetValue(dep, out var discoverer) ?? false)
                {
                    var dependencies = discoverer.DiscoverDependencies(project);

                    foreach (var dependencyName in dependencies.Select(s => Path.GetFileNameWithoutExtension(s)))
                    {
                        if (this.config.TryGetValue(dependencyName, out dependency))
                        {
                            dependency!.Dependents.Add(project.Name);
                        }
                    }
                }
            }
        }
    }

    public IReadOnlySet<Project> GetChangedProjects(IEnumerable<string> changes)
    {
        var result = FindShallowChangedProjects(changes);

        bool countChanged;
        do
        {
            int oldCount = result.Count;

            var copy = result.ToList();
            foreach (var item in config.Values.SelectMany(p => p.Dependents))
            {
                if (config.TryGetValue(item, out var newProj))
                {
                    result.Add(newProj);
                }
            }

            countChanged = result.Count > oldCount;
        } while (countChanged);

        return result;
    }

    private HashSet<Project> FindShallowChangedProjects(IEnumerable<string> changes)
    {
        HashSet<Project> projects = [];

        foreach (var changedItem in changes.Select(c => c.Replace('\\', '/')))
        {
            foreach (var projectItem in config.Values)
            {
                if (changedItem.StartsWith(projectItem.Path.Replace('\\', '/'), StringComparison.InvariantCulture))
                {
                    projects.Add(projectItem);
                }
            }
        }

        return projects;
    }
}
