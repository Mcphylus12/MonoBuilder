using MonoBuilder;

namespace Tests;

public class ChangeResolverTests
{
    [Fact]
    public void SingleFileResolves()
    {
        var config = new Config()
        {
            Projects = [
                new Project()
                    {
                        Name = "TestP",
                        Path = "SomeProjectDir/TestP"
                    }
                ]
        };

        var resolver = new ChangeResolver(config);

        List<string> input = ["SomeProjectDir/TestP/testfile.txt"];

        var projects = resolver.GetChangedProjects(input);

        Assert.Contains("TestP", projects.Select(p => p.Name));
    }

    [Fact]
    public void TwoChangesInSameProjectDontDuplicate()
    {
        var config = new Config()
        {
            Projects = [
                new Project()
                    {
                        Name = "TestP",
                        Path = "SomeProjectDir/TestP"
                    }
                ]
        };

        var resolver = new ChangeResolver(config);

        List<string> input = ["SomeProjectDir/TestP/testfile.txt", "SomeProjectDir/TestP/testfile222.txt"];

        var projects = resolver.GetChangedProjects(input);

        Assert.Single(projects);
    }

    [Fact]
    public void DependencyResolves()
    {
        var config = new Config()
        {
            Projects = [
                new Project()
                    {
                        Name = "TestP",
                        Path = "SomeProjectDir/TestP"
                    },
                    new Project()
                    {
                        Name = "2P",
                        Path = "SomeOther/Helo",
                        Dependencies = [
                            "TestP"
                            ]
                    }
                ],
        };

        var resolver = new ChangeResolver(config);

        List<string> input = ["SomeProjectDir/TestP/testfile.txt"];

        var projects = resolver.GetChangedProjects(input);

        Assert.Contains("2P", projects.Select(p => p.Name));
    }
}
