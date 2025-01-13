namespace MisterGames.Common.Tick {

    public interface ITimeSourceApi {
        
        int SubscribersCount { get; }

        void Tick();
        void Reset();
    }

}
