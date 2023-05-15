namespace MisterGames.Common.Dependencies {
    
    public interface IDependencyResolver {
        void AddDependency<T>();
        T ResolveDependency<T>();
    }
    
}
