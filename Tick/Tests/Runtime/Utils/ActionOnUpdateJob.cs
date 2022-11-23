using System;
using MisterGames.Tick.Core;
using MisterGames.Tick.Jobs;

namespace Utils {

    public class ActionOnUpdateJob : IJob, IUpdate {

        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        private readonly Action<ActionOnUpdateJob> _action;

        public ActionOnUpdateJob(Action<ActionOnUpdateJob> action = null) {
            _action = action;
        }

        public void Start() {
            IsStarted = true;
        }

        public void Stop() {
            IsStopped = true;
        }

        public void OnUpdate(float dt) {
            _action?.Invoke(this);
        }

        public void ForceComplete() {
            IsCompleted = true;
        }
    }

}
