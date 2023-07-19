namespace MisterGames.Common.Dependencies {

    public interface IDependencyContainer {
        IDependencyContainer CreateBucket(object source);
        IDependencyContainer Add<T>() where T : class;
    }
}
