using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private Transform _head;
        [SerializeField] private CameraContainer _cameraContainer;

        public Vector3 Position {
            get => _head.position;
            set => _cameraContainer.SetPositionOffset(_cameraStateKey, value - _head.position);
        }

        public Quaternion Rotation {
            get => _head.rotation;
            set => _cameraContainer.SetRotationOffset(_cameraStateKey, value * Quaternion.Inverse(_head.rotation));
        }

        private CameraStateKey _cameraStateKey;

        private void OnEnable() {
            _cameraStateKey = _cameraContainer.CreateState(this);
        }

        private void OnDisable() {
            _cameraContainer.RemoveState(_cameraStateKey);
        }

        public void Move(Vector3 delta) {
            _cameraContainer.AddPositionOffset(_cameraStateKey, delta);
        }

        public void Rotate(Quaternion delta) {
            _cameraContainer.AddRotationOffset(_cameraStateKey, delta);
        }
    }

}
