using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class EachFrameWhileJob : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly Func<bool> _actionWhile;
        private bool _isUpdating;
        private bool _isCompleted;

        public EachFrameWhileJob(Func<bool> actionWhile) {
            _actionWhile = actionWhile;
        }
        
        public void Start() {
            _isUpdating = !_isCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void Reset() {
            _isUpdating = false;
            _isCompleted = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (_actionWhile.Invoke()) return;

            _isCompleted = true;
            _isUpdating = false;
        }
    }
    
}
