namespace MisterGames.Common.Dependencies {
    
    public interface IDependency {
        void OnAddDependencies(IDependencyResolver resolver);

        void OnResolveDependencies(IDependencyResolver resolver);
    }

}
