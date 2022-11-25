using System;
using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {

    internal sealed class EachFrameJob : IJob, IUpdate {

        public bool IsCompleted => false;
        public float Progress => 0f;

        private readonly Action _action;
        private bool _isUpdating;

        public EachFrameJob(Action action) {
            _action = action;
        }

        public void Start() {
            _isUpdating = true;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _action.Invoke();
        }
    }

    internal sealed class EachFrameWhileJob : IJob, IUpdate {

        public bool IsCompleted => !_canContinue;
        public float Progress => _canContinue ? 0f : 1f;

        private readonly Func<bool> _actionWhile;
        private bool _isUpdating;
        private bool _canContinue = true;

        public EachFrameWhileJob(Func<bool> actionWhile) {
            _actionWhile = actionWhile;
        }
        
        public void Start() {
            _isUpdating = _canContinue;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _canContinue = _actionWhile.Invoke();
            _isUpdating = _canContinue;
        }
    }
}
