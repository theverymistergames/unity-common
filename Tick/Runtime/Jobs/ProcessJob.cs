using System;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class ProcessJob : IJob, IUpdate {

        public bool IsCompleted => _isCompleted;

        private readonly Func<float> _getProcess;
        private readonly Action<float> _action;

        private bool _isCompleted = false;
        private bool _isUpdating = false;

        public ProcessJob(Func<float> getProcess, Action<float> action) {
            _action = action;
            _getProcess = getProcess;
        }
        
        public void Start() {
            if (_isCompleted) {
                _isUpdating = false;
                return;
            }

            float process = Mathf.Clamp01(_getProcess.Invoke());
            _action.Invoke(process);

            _isUpdating = process < 1f;
            _isCompleted = !_isUpdating;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            float process = Mathf.Clamp01(_getProcess.Invoke());
            _action.Invoke(process);

            if (process < 1f) return;

            _isCompleted = true;
            _isUpdating = false;
        }
    }
    
}
