namespace MisterGames.Common.Tick {

    public interface ITimeSource {
        
        float DeltaTime { get; }
        float TimeScale { get; set; }
        float ScaledTime { get; }
        bool IsPaused { get; set; }

        bool Subscribe(IUpdate sub);
        bool Unsubscribe(IUpdate sub);
    }

}
