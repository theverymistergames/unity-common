namespace MisterGames.Common.Dependencies {

    public interface IDependencySetter {
        void SetValue<T>(T value) where T : class;
    }

}
