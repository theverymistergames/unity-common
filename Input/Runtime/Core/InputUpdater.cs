using MisterGames.Tick.Core;
using MisterGames.Input.Global;
using UnityEngine;

namespace MisterGames.Input.Core {

    internal sealed class InputUpdater : MonoBehaviour, IUpdate {

        [SerializeField] private PlayerLoopStage _timeSourceStage = PlayerLoopStage.PreUpdate;
        [SerializeField] private InputChannel _inputChannel;

        private ITimeSource _timeSource => TimeSources.Get(_timeSourceStage);

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
            _timeSource.Subscribe(this);
        }

        private void OnDisable() {
            GlobalInput.Disable();
            _inputChannel.Deactivate();
            _timeSource.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            _inputChannel.DoUpdate(dt);
        }
    }

}
