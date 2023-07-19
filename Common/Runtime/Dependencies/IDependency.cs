namespace MisterGames.Common.Dependencies {
    
    public interface IDependency {
        void OnSetupDependencies(IDependencyContainer container);
        void OnResolveDependencies(IDependencyResolver resolver);
    }

}
