using MisterGames.Tick.TimeProviders;

namespace Utils {

    public readonly struct ConstantTimeProvider : ITimeProvider {

        public float UnscaledDeltaTime { get; }

        public ConstantTimeProvider(float value) {
            UnscaledDeltaTime = value;
        }
    }

}
