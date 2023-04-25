using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CameraController : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        public Vector3 Position => _transform.position;
        public Quaternion Rotation => _transform.rotation;

        private CameraOffset _baseCameraOffset;
        private CameraOffset _resultCameraOffset;

        private readonly Dictionary<object, CameraOffset> _cameraOffsetMap = new Dictionary<object, CameraOffset>();

        private readonly struct CameraOffset {

            public static readonly CameraOffset Default = new CameraOffset(Vector3.zero, Quaternion.identity, 0f);
            
            public readonly Vector3 position;
            public readonly Quaternion rotation;
            public readonly float fov;

            public CameraOffset(Vector3 position, Quaternion rotation, float fov) {
                this.position = position;
                this.rotation = rotation;
                this.fov = fov;
            }
            
            public CameraOffset WithPosition(Vector3 value) => new CameraOffset(value, rotation, fov);
            public CameraOffset WithRotation(Quaternion value) => new CameraOffset(position, value, fov);
            public CameraOffset WithFovOffset(float value) => new CameraOffset(position, rotation, value);
        }

        private void Awake() {
            _baseCameraOffset = new CameraOffset(_transform.localPosition, _transform.localRotation, _camera.fieldOfView);
        }

        private void OnDestroy() {
            _cameraOffsetMap.Clear();
        }

        public void RegisterInteractor(object interactor) {
            if (_cameraOffsetMap.ContainsKey(interactor)) return;

            _cameraOffsetMap.Add(interactor, CameraOffset.Default);
        }

        public void UnregisterInteractor(object interactor) {
            if (!_cameraOffsetMap.ContainsKey(interactor)) return;

            _cameraOffsetMap.Remove(interactor);

            InvalidateResultOffset();
            InvalidateResultRotation();
            InvalidateResultFovOffset();

            ApplyCameraParameters();
        }

        public void AddPositionOffset(object interactor, Vector3 offsetDelta) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithPosition(data.position + offsetDelta);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void SetPositionOffset(object interactor, Vector3 offset) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithPosition(offset);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void ResetPositionOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithPosition(Vector3.zero);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void AddRotationOffset(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithRotation(data.rotation * rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }

        public void SetRotationOffset(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithRotation(rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void ResetRotationOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithRotation(Quaternion.identity);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithFovOffset(fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithFovOffset(data.fov + fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraOffsetMap[interactor];
            _cameraOffsetMap[interactor] = data.WithFovOffset(0f);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void InvalidateResultOffset() {
            var position = Vector3.zero;
            foreach (var data in _cameraOffsetMap.Values) {
                position += data.position;
            }

            _resultCameraOffset = _resultCameraOffset.WithPosition(position);
        }
        
        private void InvalidateResultRotation() {
            var rotation = Quaternion.identity;
            foreach (var data in _cameraOffsetMap.Values) {
                rotation *= data.rotation;
            }

            _resultCameraOffset = _resultCameraOffset.WithRotation(rotation);
        }
        
        private void InvalidateResultFovOffset() {
            float fov = 0f;
            foreach (var data in _cameraOffsetMap.Values) {
                fov += data.fov;
            }

            _resultCameraOffset = _resultCameraOffset.WithFovOffset(fov);
        }
        
        private void ApplyCameraParameters() {
            _transform.localPosition = _baseCameraOffset.position + _resultCameraOffset.position;
            _transform.localRotation = _baseCameraOffset.rotation * _resultCameraOffset.rotation;
            _camera.fieldOfView = _baseCameraOffset.fov + _resultCameraOffset.fov;
        }

        private bool CheckInteractorIsRegistered(object interactor) {
            if (_cameraOffsetMap.ContainsKey(interactor)) return true;

            Debug.LogWarning($"{nameof(CameraController)}: Not registered interactor {interactor} is trying to interact.");
            return false;
        }
    }

}
