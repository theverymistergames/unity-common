using System;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class InstantJob : IJob {

        public bool IsCompleted => true;

        private readonly Action _action;

        public InstantJob(Action action) {
            _action = action;
        }

        public void Start() {
            _action.Invoke();
        }

        public void Stop() { }

        public void Pause() { }

        public void Resume() { }
    }
    
}
