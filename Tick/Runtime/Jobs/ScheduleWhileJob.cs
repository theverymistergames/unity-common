using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class ScheduleWhileJob : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly Func<bool> _actionWhile;
        private readonly float _period;

        private float _timer;
        private bool _isUpdating;
        private bool _isCompleted;

        public ScheduleWhileJob(float period, Func<bool> actionWhile) {
            _actionWhile = actionWhile;
            _period = period;
        }
        
        public void Start() {
            _isUpdating = !_isCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _timer += dt;
            if (_timer < _period) return;

            _timer = 0f;
            if (_actionWhile.Invoke()) return;

            _isCompleted = true;
        }
    }
    
}
