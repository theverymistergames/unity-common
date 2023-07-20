namespace MisterGames.Common.Dependencies {

    public interface IDependencyBucket {
        IDependencyBucket Add<T>() where T : class;
    }
    
}
