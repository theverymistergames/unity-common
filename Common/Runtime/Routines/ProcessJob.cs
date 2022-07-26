using System;

namespace MisterGames.Common.Routines {
    
    internal sealed class ProcessJob : IJob, IUpdate {

        public Action OnStop { private get; set; } = null;

        private readonly TimeDomain _timeDomain;
        private readonly Func<float> _getProcess;
        private readonly Action<float> _action;

        private bool _canStart = false;
        private bool _isStarted = false;
        private bool _isPaused = false;

        private ProcessJob() { }
            
        public ProcessJob(TimeDomain timeDomain, Func<float> getProcess, Action<float> action) {
            _timeDomain = timeDomain;
            _action = action;
            _getProcess = getProcess;
        }
        
        public void Start() {
            if (_isStarted) return;

            float process = _getProcess.Invoke();
            _canStart = process < 1f;
            _isStarted = true;
                
            if (!_canStart) {
                _action.Invoke(1f);
                Stop();
                return;
            }
                
            _action.Invoke(process);
            _timeDomain.SubscribeUpdate(this);
        }

        public void Stop() {
            if (!_isStarted) return;
                
            if (_canStart) _timeDomain.UnsubscribeUpdate(this);
            OnStop?.Invoke();
            _isStarted = false;
        }

        public void Pause() {
            if (!_isStarted || _isPaused) return;
            if (_canStart) _timeDomain.UnsubscribeUpdate(this);
            _isPaused = true;
        }

        public void Resume() {
            if (!_isStarted || !_isPaused) return;
            if (_canStart) _timeDomain.SubscribeUpdate(this);
            _isPaused = false;
        }

        void IUpdate.OnUpdate(float dt) {
            float process = _getProcess.Invoke();
                
            if (process >= 1f) { 
                _action.Invoke(1f);
                Stop();
                return;
            }

            _action.Invoke(process);
        }
    }
    
}