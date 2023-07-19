namespace MisterGames.Common.Dependencies {
    
    public interface IDependencyResolver {
        IDependencyResolver Resolve<T>(out T dependency) where T : class;
    }
}
