using System;

namespace MisterGames.Common.Routines {
    
    internal sealed class EachFrameWhileJob : IJob, IUpdate {

        public Action OnStop { private get; set; } = null;

        private readonly TimeDomain _timeDomain;
        private readonly Func<float, bool> _actionWhile;

        private bool _isStarted = false;
        private bool _isPaused = false;

        private EachFrameWhileJob() { }
            
        public EachFrameWhileJob(TimeDomain timeDomain, Func<float, bool> actionWhile) {
            _timeDomain = timeDomain;
            _actionWhile = actionWhile;
        }
        
        public void Start() {
            if (_isStarted) return;
                
            _timeDomain.SubscribeUpdate(this);

            _isStarted = true;
        }

        public void Stop() {
            if (!_isStarted) return;
                
            _timeDomain.UnsubscribeUpdate(this);

            OnStop?.Invoke();
            _isStarted = false;
        }

        public void Pause() {
            if (!_isStarted || _isPaused) return;
                
            _timeDomain.UnsubscribeUpdate(this);
            _isPaused = true;
        }

        public void Resume() {
            if (!_isStarted || !_isPaused) return;
                
            _timeDomain.SubscribeUpdate(this);
            _isPaused = false;
        }

        void IUpdate.OnUpdate(float dt) {
            bool canContinue = _actionWhile.Invoke(dt);
            if (!canContinue) Stop();
        }
    }
    
}