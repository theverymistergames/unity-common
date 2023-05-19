namespace MisterGames.Common.Dependencies {
    
    public interface IDependency {
        void OnAddDependencies(IDependencyContainer container);

        void OnResolveDependencies(IDependencyResolver resolver);
    }

}
