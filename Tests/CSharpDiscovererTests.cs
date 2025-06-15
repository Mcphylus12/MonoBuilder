using MonoBuilder;

namespace Tests;

public class CSharpDiscovererTests
{
    [Fact]
    public void DiscoversTestProject()
    {
        var sut = new CSharpDiscoverer();

        var deps = sut.DiscoverDependencies(new Project
        {
            Name = "test",
            Path = "../../../../TestApi"
        });

        Console.WriteLine("gggr");
    }

    [Fact]
    public void DiscovererResovesDependents()
    {
        var config = new Config()
        {
            Projects = [
                    new Project()
                    {
                        Name = "TestApi",
                        Path = "../../../../TestApi",
                        Dependencies = ["CSharpDiscoverer"]
                    },
                    new Project()
                    {
                        Name = "TestLib",
                        Path = "../../../../TestLib",
                    }
                ]
        };

        var resolver = new ChangeResolver(config);

        List<string> input = ["../../../../TestLib/TestP/testfile.txt"];

        var projects = resolver.GetChangedProjects(input);

        Assert.Contains("TestApi", projects.Select(p => p.Name));
    }
}
