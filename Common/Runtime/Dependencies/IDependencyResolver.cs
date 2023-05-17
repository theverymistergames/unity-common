namespace MisterGames.Common.Dependencies {
    
    public interface IDependencyResolver {
        void AddDependency<T>(object source);
        T ResolveDependency<T>();
    }
    
}
