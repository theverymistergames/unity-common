namespace MisterGames.Common.Dependencies {

    public interface IDependencyOverride {
        void SetDependenciesOfType<T>(T value) where T : class;
        bool TryResolveDependencyOverride<T>(out T value);
    }

}
