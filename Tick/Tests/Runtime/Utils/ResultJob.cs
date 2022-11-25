using System;
using MisterGames.Tick.Jobs;

namespace Utils {

    public class ResultJob<R> : IJob<R> {

        public R Result { get; private set; }

        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        public float Progress => IsCompleted ? 1f : 0f;

        private readonly Action<ResultJob<R>> _actionOnStart;

        public ResultJob(Action<ResultJob<R>> actionOnStart = null) {
            _actionOnStart = actionOnStart;
        }

        public void Start() {
            _actionOnStart?.Invoke(this);
            IsStarted = true;
        }
        public void Stop() {
            IsStopped = true;
        }

        public void ForceComplete(R result = default) {
            IsCompleted = true;
            Result = result;
        }
    }

}
