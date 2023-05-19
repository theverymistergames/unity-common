namespace MisterGames.Common.Dependencies {
    
    public interface IDependencyResolver {
        T ResolveDependency<T>();
    }
    
}
