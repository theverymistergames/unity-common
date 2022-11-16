using MisterGames.Tick.Core;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Core {

    internal sealed class InputUpdater : MonoBehaviour, IUpdate {

        [SerializeField] private TimeDomain _timeDomain;
        [SerializeField] private InputChannel _inputChannel;

        private void Awake() {
            GlobalInput.Init();
            _inputChannel.Init();
        }

        private void OnDestroy() {
            GlobalInput.Terminate();
            _inputChannel.Terminate();
        }

        private void OnEnable() {
            GlobalInput.Enable();
            _inputChannel.Activate();
            _timeDomain.Source.Subscribe(this);
        }

        private void OnDisable() {
            GlobalInput.Disable();
            _inputChannel.Deactivate();
            _timeDomain.Source.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            _inputChannel.DoUpdate(dt);
        }
    }

}
