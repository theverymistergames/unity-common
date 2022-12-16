using MisterGames.Tick.Core;

namespace Utils {

    public class ConstantDeltaTimeProvider : IDeltaTimeProvider {

        public float DeltaTime { get; }

        public ConstantDeltaTimeProvider(float value) {
            DeltaTime = value;
        }
    }

}
