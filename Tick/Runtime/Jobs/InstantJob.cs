using System;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class InstantJob : IJob {

        public bool IsCompleted => _isCompleted;

        private readonly Action _action;
        private bool _isCompleted;

        public InstantJob(Action action) {
            _action = action;
        }

        public void Start() {
            if (_isCompleted) return;

            _action.Invoke();
            _isCompleted = true;
        }

        public void Stop() { }
    }
    
}
