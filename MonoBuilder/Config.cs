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

public class ChangeResolver
{
    private readonly Dictionary<string, Project> config;

    public ChangeResolver(Config config)
    {
        this.config = config.Projects.ToDictionary(p => p.Name);
    }

    public IReadOnlySet<Project> GetChangedProjects(IEnumerable<string> changes)
    {
        var result = FindShallowChangedProjects(changes);

        bool countChanged;
        do
        {
            int oldCount = result.Count;

            var copy = result.ToList();
            foreach (var item in copy.SelectMany(p => p.Dependencies))
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
