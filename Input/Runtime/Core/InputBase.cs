using System;
using UnityEngine;

namespace MisterGames.Input.Core {
    
    public abstract class InputBase : ScriptableObject {
        
        public event Action OnActivate = delegate {  };
        public event Action OnDeactivate = delegate {  };

        protected Stage CurrentStage { get; private set; } = Stage.Terminated;

        internal void Init() {
            if (CurrentStage != Stage.Terminated) return;
            
            CurrentStage = Stage.Initialized;
            OnInit();
        }

        internal void Terminate() {
            if (CurrentStage == Stage.Terminated) return;
            
            CurrentStage = Stage.Terminated;
            OnTerminate();
        }
        
        internal void Activate() {
            if (CurrentStage == Stage.Active) return;
            if (CurrentStage == Stage.Terminated) Init();
            
            CurrentStage = Stage.Active;
            OnActivated();
            OnActivate.Invoke();
        }

        internal void Deactivate() {
            if (CurrentStage == Stage.Inactive) return;
            if (CurrentStage == Stage.Terminated) Init();
            
            CurrentStage = Stage.Inactive;
            OnDeactivated();
            OnDeactivate.Invoke();
        }

        internal void DoUpdate(float dt) {
            if (CurrentStage != Stage.Active) return;
            OnUpdate(dt);
        }
        
        protected abstract void OnInit();
        
        protected abstract void OnTerminate();
        
        protected abstract void OnUpdate(float dt);

        protected abstract void OnActivated();
        
        protected abstract void OnDeactivated();

        protected enum Stage {
            Initialized,
            Active,
            Inactive,
            Terminated
        }
    }
    
}