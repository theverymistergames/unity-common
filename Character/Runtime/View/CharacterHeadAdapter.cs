using MisterGames.Character.Access;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CharacterAccess _characterAccess;

        public Vector3 Position {
            get => _cameraController.PositionOffset;
            set => _cameraController.SetPositionOffset(this, value);
        }

        public Quaternion Rotation {
            get => _cameraController.Rotation;
            set => _cameraController.SetRotation(this, value);
        }

        private CameraController _cameraController;

        private void Awake() {
            _cameraController = _characterAccess.CameraController;
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
