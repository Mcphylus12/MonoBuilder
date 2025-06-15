namespace MonoBuilder;

public interface IDiscoverer
{
    string[] DiscoverDependencies(Project item);
}