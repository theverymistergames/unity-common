using System;
using MisterGames.Tick.Jobs;

namespace Utils {

    public class ActionOnStartJob : IJob {

        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        private readonly Action<ActionOnStartJob> _action;

        public ActionOnStartJob(Action<ActionOnStartJob> action = null) {
            _action = action;
        }

        public void Start() {
            _action?.Invoke(this);
            IsStarted = true;
        }
        public void Stop() {
            IsStopped = true;
        }

        public void ForceComplete() {
            IsCompleted = true;
        }
    }

}
