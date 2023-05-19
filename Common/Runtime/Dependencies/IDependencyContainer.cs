namespace MisterGames.Common.Dependencies {

    public interface IDependencyContainer {
        void AddDependency<T>(object source);
    }

}
