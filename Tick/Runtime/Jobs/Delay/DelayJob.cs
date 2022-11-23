﻿using MisterGames.Tick.Core;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class DelayJob : IJob, IUpdate {

        public bool IsCompleted => _timer >= _delay;

        private readonly float _delay;

        private float _timer;
        private bool _isUpdating;

        public DelayJob(float delaySeconds) {
            _delay = delaySeconds;
        }

        public void Start() {
            _isUpdating = _timer < _delay;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _timer += dt;
            _isUpdating = _timer < _delay;
        }
    }
    
}