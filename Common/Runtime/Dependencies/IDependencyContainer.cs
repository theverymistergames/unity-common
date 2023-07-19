namespace MisterGames.Common.Dependencies {

    public interface IDependencyContainer {
        IDependencyContainer Register(object source);
        IDependencyContainer Add<T>() where T : class;
    }

}
