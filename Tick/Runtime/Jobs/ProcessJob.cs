using System;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Tick.Jobs {
    
    internal sealed class ProcessJob : IJob, IUpdate {

        public bool IsCompleted => _process >= 1f;

        private readonly Func<float> _getProcess;
        private readonly Action<float> _action;

        private float _process;
        private bool _isUpdating = false;

        public ProcessJob(Func<float> getProcess, Action<float> action) {
            _action = action;
            _getProcess = getProcess;
        }
        
        public void Start() {
            _isUpdating = _process < 1f;
        }

        public void Stop() {
            _isUpdating = false;
        }

        public void Reset() {
            _isUpdating = false;
            _process = 0f;
        }

        public void OnUpdate(float dt) {
            if (!_isUpdating) return;

            _process = Mathf.Clamp01(_getProcess.Invoke());
            _action.Invoke(_process);

            _isUpdating = _process < 1;
        }
    }
    
}
