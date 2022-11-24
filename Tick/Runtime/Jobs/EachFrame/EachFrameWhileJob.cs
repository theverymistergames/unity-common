using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class EachFrameWhileJob : IJob, IUpdate {

        public bool IsCompleted { get; private set; }

        private readonly Func<bool> _actionWhile;
        private bool _isUpdating;

        public EachFrameWhileJob(Func<bool> actionWhile) {
            _actionWhile = actionWhile;
        }
        
        public void Start() {
            _isUpdating = !IsCompleted;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            if (_actionWhile.Invoke()) return;

            IsCompleted = true;
            _isUpdating = false;
        }
    }
    
}
