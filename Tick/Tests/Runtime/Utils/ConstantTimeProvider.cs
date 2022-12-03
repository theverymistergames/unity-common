using MisterGames.Tick.Core;

namespace Utils {

    public class ConstantTimeProvider : ITimeProvider {

        public float DeltaTime { get; }
        public float TimeScale { get; set; } = 1f;

        public ConstantTimeProvider(float value) {
            DeltaTime = value;
        }
    }

}
