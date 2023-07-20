namespace MisterGames.Common.Dependencies {

    public interface IDependencyContainer {
        IDependencyBucket CreateBucket(object source);
    }

}
