using System;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {

    internal sealed class ScheduleJob : IJob, IUpdate {

        public bool IsCompleted => false;
        public float Progress => 0f;

        private readonly Action _action;
        private readonly float _period;

        private float _timer;
        private bool _isUpdating;

        public ScheduleJob(float period, Action action) {
            _action = action;
            _period = period;
        }

        public void Start() {
            _isUpdating = true;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _timer += dt;
            if (_timer < _period) return;

            _timer = 0f;
            _action.Invoke();
        }
    }

    internal sealed class ScheduleWhileJob : IJob, IUpdate {

        public bool IsCompleted => !_canContinue;
        public float Progress => _canContinue ? 0f : 1f;

        private readonly Func<bool> _actionWhile;
        private readonly float _period;

        private float _timer;
        private bool _isUpdating;
        private bool _canContinue = true;

        public ScheduleWhileJob(float period, Func<bool> actionWhile) {
            _actionWhile = actionWhile;
            _period = period;
        }
        
        public void Start() {
            _isUpdating = _canContinue;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _timer += dt;
            if (_timer < _period) return;

            _timer = 0f;
            _canContinue = _actionWhile.Invoke();
            _isUpdating = _canContinue;
        }
    }

    internal sealed class ScheduleTimesJob : IJob, IUpdate {

        public bool IsCompleted => _timesTimer >= _times;
        public float Progress => _times <= 0
            ? 1f
            : Mathf.Clamp01((float) _timesTimer / _times);

        private readonly Action _action;
        private readonly float _period;
        private readonly int _times;

        private float _periodTimer;
        private int _timesTimer;
        private bool _isUpdating;

        public ScheduleTimesJob(float period, int times, Action action) {
            _period = period;
            _times = times;
            _action = action;
        }

        public void Start() {
            _isUpdating = _timesTimer < _times;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _periodTimer += dt;
            if (_periodTimer < _period) return;

            _periodTimer = 0f;
            _timesTimer++;
            _action.Invoke();
            _isUpdating = _timesTimer < _times;
        }
    }

    internal sealed class ScheduleTimesWhileJob : IJob, IUpdate {

        public bool IsCompleted => !_canContinue || _timesTimer >= _times;
        public float Progress => !_canContinue || _times <= 0
            ? 1f
            : Mathf.Clamp01((float) _timesTimer / _times);

        private readonly Func<bool> _actionWhile;
        private readonly float _period;
        private readonly int _times;

        private float _periodTimer;
        private int _timesTimer;
        private bool _isUpdating;
        private bool _canContinue = true;

        public ScheduleTimesWhileJob(float period, int times, Func<bool> actionWhile) {
            _period = period;
            _times = times;
            _actionWhile = actionWhile;
        }

        public void Start() {
            _isUpdating = _canContinue && _timesTimer < _times;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _periodTimer += dt;
            if (_periodTimer < _period) return;

            _periodTimer = 0f;
            _timesTimer++;
            _canContinue = _actionWhile.Invoke();
            _isUpdating = _canContinue && _timesTimer < _times;
        }
    }
    
}
