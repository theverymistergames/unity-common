using UnityEngine;

namespace MisterGames.Character.Core2 {

    public class CharacterViewAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CharacterAccess _characterAccess;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private Transform _body;

        public Vector3 Position {
            get => _cameraController.PositionOffset;
            set => _cameraController.SetPositionOffset(this, value);
        }

        public Quaternion Rotation {
            get => _cameraController.Rotation;
            set {
                var eulerAngles = value.eulerAngles;
                _cameraController.SetRotation(this, Quaternion.Euler(eulerAngles.x, 0f, eulerAngles.z));
                _body.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
            }
        }

        public void Move(Vector3 delta) {
            _cameraController.AddPositionOffset(this, delta);
        }

        public void Rotate(Quaternion delta) {
            var eulerAngles = delta.eulerAngles;
            _cameraController.Rotate(this, Quaternion.Euler(eulerAngles.x, 0f, eulerAngles.z));
            _body.Rotate(0f, eulerAngles.y, 0f);
        }
    }

}
