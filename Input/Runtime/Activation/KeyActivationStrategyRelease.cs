using System;

namespace MisterGames.Input.Activation {

    [Serializable]
    public sealed class KeyActivationStrategyRelease : IKeyActivationStrategy {

        public Action OnUse { set => _onUse = value; }
        private Action _onUse = delegate {  };

        public void OnPressed() { }

        public void OnReleased() {
            _onUse.Invoke();
        }

        public void Interrupt() { }
        public void OnUpdate(float dt) { }
    }

}
