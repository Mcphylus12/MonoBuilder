namespace MonoBuilder;

public class ChangeResolver
{
    private readonly Dictionary<string, Project> config;
    private readonly Dictionary<string, IDiscoverer> discoverers;
    private readonly string currentDir;
    private readonly Dictionary<string, List<string>> dependents = [];

    public ChangeResolver(Config config)
    {
        this.config = config.Projects.ToDictionary(p => p.Name);
        this.discoverers = new Dictionary<string, IDiscoverer>
        {
            ["CSharpDiscoverer"] = new CSharpDiscoverer()
        };
        currentDir = Directory.GetCurrentDirectory();
        InvertDependencies();
    }

    private void InvertDependencies()
    {
        var generatedProjects = new Dictionary<string, Project>();

        foreach (var (project, dependencyName) in config.Values.SelectMany(p => p.Dependencies, (project, dependency) => (project, dependency)))
        {
            if (this.discoverers?.TryGetValue(dependencyName, out var discoverer) ?? false)
            {
                var dependencies = discoverer.DiscoverDependencies(project);

                InvertDiscoveredDependencies(project, dependencies, generatedProjects);
            }
            else
            {
                TryAddDependent(project, dependencyName);
            }
        }

        foreach (var item in generatedProjects.Values)
        {
            config[item.Name] = item;
        }
    }

    private void InvertDiscoveredDependencies(Project project, string[] dependencies, Dictionary<string, Project> newProjects)
    {
        foreach (var dependencyName in dependencies)
        {
            var expectedDependencyProjectName = Path.GetFileNameWithoutExtension(dependencyName);

            if (!TryAddDependent(project, expectedDependencyProjectName))
            {
                if (!newProjects.ContainsKey(dependencyName))
                {
                    newProjects[dependencyName] = new Project
                    {
                        Name = expectedDependencyProjectName,
                        Path = Path.GetRelativePath(currentDir, Path.GetDirectoryName(dependencyName)!)
                    };
                }

                dependents.AddToList(expectedDependencyProjectName, project.Name);
            }
        }
    }

    private bool TryAddDependent(Project project, string dependencyName)
    {
        if (this.config.ContainsKey(dependencyName))
        {
            dependents.AddToList(dependencyName, project.Name);
            return true;
        }

        return false;
    }

    public IReadOnlySet<Project> GetChangedProjects(IEnumerable<string> changes)
    {
        var projects = FindChangedProjects(changes);
        var depedentNames = AddDependents(projects.Select(r => r.Name));
        var depedents = depedentNames
            .Select(dep => config.GetValueOrDefault(dep))
            .Where(d => d is not null);

        foreach (var dependent in depedents)
        {
            projects.Add(dependent!);
        }
        return projects;
    }

    private List<string> AddDependents(IEnumerable<string> projects)
    {
        var result = new List<string>();
        foreach (var project in projects)
        {
            if (dependents.TryGetValue(project, out var projectDependents))
            {
                result.AddRange(projectDependents);

                result.AddRange(AddDependents(projectDependents));
            }
        }

        return result;
    }

    private HashSet<Project> FindChangedProjects(IEnumerable<string> changes)
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

public static class DictionaryExtensions
{
    public static void AddToList<K, V>(this Dictionary<K, List<V>> dictionary, K key, V value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary[key] = new List<V>();
        }

        dictionary[key].Add(value);
    }
}