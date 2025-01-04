using MisterGames.Common.Tick;

namespace Utils {

    public class ConstantDeltaTimeProvider : IDeltaTimeProvider {

        public float DeltaTime { get; }

        public ConstantDeltaTimeProvider(float value) {
            DeltaTime = value;
        }
    }

}
