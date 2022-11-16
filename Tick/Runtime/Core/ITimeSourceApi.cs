using MisterGames.Tick.TimeProviders;

namespace MisterGames.Tick.Core {

    public interface ITimeSourceApi {

        void Initialize(ITimeProvider timeProvider);
        void DeInitialize();

        void Enable();
        void Disable();

        void Tick();
        void UpdateDeltaTime();
    }

}
