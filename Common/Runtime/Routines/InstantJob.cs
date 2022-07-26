using System;

namespace MisterGames.Common.Routines {
    
    internal sealed class InstantJob : IJob {

        public Action OnStop { private get; set; } = null;
        private readonly Action _action;
        private bool _isStarted = false;

        private InstantJob() {}
            
        public InstantJob(Action action) {
            _action = action;
        }
        
        public void Start() {
            if (_isStarted) return;
                
            _action?.Invoke();
            _isStarted = true;
                
            Stop();
        }

        public void Stop() {
            if (!_isStarted) return;
                
            OnStop?.Invoke();
            _isStarted = false;
        }

        public void Pause() { }

        public void Resume() { }
    }
    
}