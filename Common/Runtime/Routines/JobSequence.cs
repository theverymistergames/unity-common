using System;

namespace MisterGames.Common.Routines {
    
    public sealed class JobSequence : IJob {

        public Action OnStop { private get; set; } = null;

        private IJob _currentJob;
        private IJob _lastAddedJob;

        private bool _isStarted = false;
        private bool _isPaused = false;

        private JobSequence() { }
        
        internal JobSequence(IJob job) {
            _currentJob = job;
            _lastAddedJob = job;
            
            _isStarted = false;
            _isPaused = false;
            
            OnStop = null;
            _lastAddedJob.OnStop = Stop;
        }
            
        public JobSequence Then(IJob job) {
            _lastAddedJob.OnStop = () => {
                _currentJob = job;
                _currentJob.Start();
            };
                
            _lastAddedJob = job;
            _lastAddedJob.OnStop = Stop;

            return this;
        }
            
        public JobSequence Then(Action action) {
            return Then(Jobs.Instant(action));
        }

        public void Start() {
            if (_isStarted) return;
                
            _currentJob.Start();
            _isStarted = true;
        }

        public void Stop() {
            if (!_isStarted) return;
                
            _currentJob.OnStop = null;
            _currentJob.Stop();
                
            OnStop?.Invoke();
            _isStarted = false;
        }

        public void Pause() {
            if (!_isStarted || _isPaused) return;
            
            _currentJob.Pause();
            _isPaused = true;
        }

        public void Resume() {
            if (!_isStarted || !_isPaused) return;
            
            _currentJob.Resume();
            _isPaused = false;
        }
    }
    
}