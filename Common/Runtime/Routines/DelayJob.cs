using System;

namespace MisterGames.Common.Routines {
    
    internal sealed class DelayJob : IJob, IUpdate {

        public Action OnStop { private get; set; } = null;

        private readonly TimeDomain _timeDomain;
        private readonly float _seconds = 0f;
            
        private float _timer = 0f;
        private bool _isStarted = false;
        private bool _isPaused = false;

        private DelayJob() { }
            
        public DelayJob(TimeDomain timeDomain, float seconds) {
            _timeDomain = timeDomain;
            _seconds = seconds;
        }
        
        public void Start() {
            if (_isStarted) return;
            _isStarted = true;
                
            if (_seconds <= 0f) {
                Stop();
                return;
            }
                
            _timeDomain.SubscribeUpdate(this);
        }

        public void Stop() {
            if (!_isStarted) return;
                
            if (_seconds > 0f) _timeDomain.UnsubscribeUpdate(this);
            OnStop?.Invoke();
            _isStarted = false;
        }

        public void Pause() {
            if (!_isStarted || _isPaused) return;
            if (_seconds > 0f) _timeDomain.UnsubscribeUpdate(this);
            _isPaused = true;
        }

        public void Resume() {
            if (!_isStarted || !_isPaused) return;
            if (_seconds > 0f) _timeDomain.SubscribeUpdate(this);
            _isPaused = false;
        }

        void IUpdate.OnUpdate(float dt) {
            _timer += dt;
            if (_timer >= _seconds) Stop();
        }
    }
    
}