using System;

namespace MisterGames.Input.Activation {

    [Serializable]
    public sealed class KeyActivationStrategyPress : IKeyActivationStrategy {

        public Action OnUse { set => _onUse = value; }
        private Action _onUse = delegate {  };

        public void OnPressed() {
            _onUse.Invoke();
        }

        public void OnReleased() { }

        public void Interrupt() { }
        public void OnUpdate(float dt) { }
    }

}
