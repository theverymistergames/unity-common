using System;

namespace MisterGames.Character.Core2.Run {

    public interface ICharacterRunPipeline {
        event Action OnStartRun;
        event Action OnStopRun;

        bool IsRunActive { get; }

        void SetEnabled(bool isEnabled);
    }

}
