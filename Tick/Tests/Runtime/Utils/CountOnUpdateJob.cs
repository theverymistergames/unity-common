using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;

namespace Utils {

    public class CountOnUpdateJob : IJob, IUpdate {

        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        public float Progress => IsCompleted ? 1f : 0f;
        public int Count { get; private set; }

        public void Start() {
            IsStarted = true;
        }

        public void Stop() {
            IsStopped = true;
        }

        public void OnUpdate(float dt) {
            Count++;
        }

        public void ForceComplete() {
            IsCompleted = true;
        }
    }

}
