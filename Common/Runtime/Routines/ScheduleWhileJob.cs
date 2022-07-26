using System;

namespace MisterGames.Common.Routines {
    
    internal sealed class ScheduleWhileJob : IJob, IUpdate {

        public Action OnStop { private get; set; } = null;

        private readonly TimeDomain _timeDomain;
        private readonly Func<bool> _actionWhile;
        private readonly float _period = 0f;

        private float _timer = 0f;

        private bool _canStart = false;
        private bool _isStarted = false;
        private bool _isPaused = false;

        private ScheduleWhileJob() { }
            
        public ScheduleWhileJob(TimeDomain timeDomain, float period, Func<bool> actionWhile) {
            _timeDomain = timeDomain;
            _actionWhile = actionWhile;
            _period = period;
        }
        
        public void Start() {
            if (_isStarted) return;
                
            _canStart = _actionWhile.Invoke();
            _isStarted = true;
                
            if (!_canStart) {
                Stop();
                return;
            }
                
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
            _timer += dt;
            if (_timer < _period) return;
                
            _timer = 0f;
            bool canContinue = _actionWhile.Invoke();
            if (!canContinue) Stop();
        }
    }
    
}