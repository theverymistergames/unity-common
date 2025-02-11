﻿namespace MisterGames.Common.Tick {

    public interface ITimeSource {
        
        float DeltaTime { get; }
        float TimeScale { get; set; }
        bool IsPaused { get; set; }

        bool Subscribe(IUpdate sub);
        bool Unsubscribe(IUpdate sub);
    }

}
