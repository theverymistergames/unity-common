using System.Collections.Generic;
using MisterGames.Tick.TimeProviders;

namespace MisterGames.Tick.Core {

    public interface ITimeSourceApi {

        IReadOnlyList<IUpdate> Subscribers { get; }

        void Initialize(ITimeProvider timeProvider);
        void DeInitialize();

        void Enable();
        void Disable();

        void Tick();
        void UpdateDeltaTime();
    }

}
