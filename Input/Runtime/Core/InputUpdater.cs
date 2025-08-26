using System;
using MisterGames.Common.Tick;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {
    
    public sealed class InputUpdater : IDisposable, IUpdate {

        private bool _initialized;
        
        public void Initialize() {
            _initialized = true;
            
            PlayerLoopStage.PreUpdate.Subscribe(this);
        }

        public void Dispose() {
            if (!_initialized) return;
            
            _initialized = false;
            
            PlayerLoopStage.PreUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            InputSystem.Update();
        }
    }
    
}