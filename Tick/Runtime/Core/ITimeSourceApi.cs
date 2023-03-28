namespace MisterGames.Tick.Core {

    public interface ITimeSourceApi {
        int SubscribersCount { get; }

        void Tick();
        void Reset();
    }

}
