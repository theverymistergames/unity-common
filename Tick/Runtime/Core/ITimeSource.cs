namespace MisterGames.Tick.Core {

    public interface ITimeSource {
        float DeltaTime { get; }
        float TimeScale { get; set; }
        bool IsPaused { get; set; }

        bool Subscribe(IUpdate sub);
        bool Unsubscribe(IUpdate sub);
    }

    public interface ITimeSourceApi {
        void Tick();
        void Reset();
    }

}
