namespace MisterGames.Common.Dependencies {

    public interface IDependencyOverride {
        void SetValue<T>(T value) where T : class;
        bool TryResolve<T>(out T value) where T : class;
    }

}
