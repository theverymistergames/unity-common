using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CameraContainer _cameraContainer;

        public Vector3 Position {
            get => _cameraContainer.Position;
            set => _cameraContainer.SetPositionOffset(this, value - _cameraContainer.Position);
        }

        public Quaternion Rotation {
            get => _cameraContainer.Rotation;
            set => _cameraContainer.SetRotationOffset(this, value * Quaternion.Inverse(_cameraContainer.Rotation));
        }

        private void OnEnable() {
            _cameraContainer.RegisterInteractor(this);
        }

        private void OnDisable() {
            _cameraContainer.UnregisterInteractor(this);
        }

        public void Move(Vector3 delta) {
            _cameraContainer.AddPositionOffset(this, delta);
        }

        public void Rotate(Quaternion delta) {
            _cameraContainer.AddRotationOffset(this, delta);
        }
    }

}
