namespace MisterGames.Tick.Core {

    public interface ITimeSource {

        float DeltaTime { get; }
        float TimeScale { get; set; }

        bool IsPaused { get; set; }

        void Subscribe(IUpdate sub);
        void Unsubscribe(IUpdate sub);
    }

}
