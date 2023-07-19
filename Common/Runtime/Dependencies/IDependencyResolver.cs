namespace MisterGames.Common.Dependencies {
    
    public interface IDependencyResolver {
        T Resolve<T>() where T : class;
    }
}
