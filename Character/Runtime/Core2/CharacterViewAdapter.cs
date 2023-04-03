using UnityEngine;

namespace MisterGames.Character.Core2 {

    public class CharacterViewAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CameraController _cameraController;

        public Vector3 Position {
            get => _cameraController.PositionOffset;
            set => _cameraController.SetPositionOffset(this, value);
        }

        public Quaternion Rotation {
            get => _cameraController.Rotation;
            set => _cameraController.SetRotation(this, value);
        }

        private void OnEnable() {
            _cameraController.RegisterInteractor(this);
        }

        private void OnDisable() {
            _cameraController.UnregisterInteractor(this);
        }

        public void Move(Vector3 delta) {
            _cameraController.AddPositionOffset(this, delta);
        }

        public void Rotate(Quaternion delta) {
            _cameraController.Rotate(this, delta);
        }
    }

}
