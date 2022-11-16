using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class DelayJob : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly float _delay;

        private float _timer;
        private bool _isUpdating;
        private bool _isCompleted;

        public DelayJob(float delaySeconds) {
            _delay = delaySeconds;
        }

        public void Start() {
            _isCompleted = _timer >= _delay;
            _isUpdating = !_isCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _timer += dt;
            if (_timer < _delay) return;

            _isCompleted = true;
            _isUpdating = false;
        }
    }
    
}
