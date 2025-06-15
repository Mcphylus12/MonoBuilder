using System.Xml.Linq;

namespace MonoBuilder;

public class CSharpDiscoverer : IDiscoverer
{
    public string[] DiscoverDependencies(Project item)
    {
        var project = Directory.GetFiles(item.Path, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
        var dir = new DirectoryInfo(item.Path);

        if (!File.Exists(project))
        {
            Console.WriteLine($"Project file not found when discovering {item.Name}");
            return [];
        }
        var result = new List<string>();

        GetReferences(project, dir, result);

        return result.Distinct().ToArray();
    }

    private static void GetReferences(string project, DirectoryInfo dir, List<string> result)
    {
        XDocument doc = XDocument.Load(project);

        var references = doc
            .Descendants()
            .Where(x => x.Name.LocalName == "ProjectReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => Path.GetFullPath(Path.Combine(dir.FullName, x!)))
            .ToList();

        result.AddRange(references);

        foreach (var reference in references)
        {
            GetReferences(reference, dir, result);
        }
    }
}
