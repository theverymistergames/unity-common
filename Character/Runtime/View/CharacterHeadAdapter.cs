using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private Transform _head;
        [SerializeField] private CameraContainer _cameraContainer;

        public Vector3 Position {
            get => _head.position;
            set => _cameraContainer.SetPositionOffset(_cameraStateId, value - _head.position);
        }

        public Quaternion Rotation {
            get => _head.rotation;
            set => _cameraContainer.SetRotationOffset(_cameraStateId, value * Quaternion.Inverse(_head.rotation));
        }

        private int _cameraStateId;

        private void OnEnable() {
            _cameraStateId = _cameraContainer.CreateState();
        }

        private void OnDisable() {
            _cameraContainer.RemoveState(_cameraStateId);
        }

        public void Move(Vector3 delta) {
            _cameraContainer.AddPositionOffset(_cameraStateId, delta);
        }

        public void Rotate(Quaternion delta) {
            _cameraContainer.AddRotationOffset(_cameraStateId, delta);
        }
    }

}
