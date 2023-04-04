using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public class CameraController : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        public Camera Camera => _camera;

        public Vector3 PositionOffset => _transform.localPosition;
        public Quaternion Rotation => _transform.localRotation;

        private CameraValues _baseCameraValues;
        private CameraValues _resultCameraValues;

        private readonly Dictionary<object, CameraValues> _data = new Dictionary<object, CameraValues>();

        private readonly struct CameraValues {

            public static readonly CameraValues Default = 
                new CameraValues(Vector3.zero, Quaternion.identity, 0f);
            
            public readonly Vector3 positionOffset;
            public readonly Quaternion rotation;
            public readonly float fovOffset;

            public CameraValues(Vector3 positionOffset, Quaternion rotation, float fovOffset) {
                this.positionOffset = positionOffset;
                this.rotation = rotation;
                this.fovOffset = fovOffset;
            }
            
            public CameraValues WithPositionOffset(Vector3 value) => new CameraValues(value, rotation, fovOffset);
            public CameraValues WithRotation(Quaternion value) => new CameraValues(positionOffset, value, fovOffset);
            public CameraValues WithFovOffset(float value) => new CameraValues(positionOffset, rotation, value);
        }

        private void Awake() {
            _baseCameraValues = new CameraValues(_transform.localPosition, _transform.localRotation, _camera.fieldOfView);
        }

        private void OnDestroy() {
            _data.Clear();
        }

        public void RegisterInteractor(object interactor) {
            if (_data.ContainsKey(interactor)) return;

            _data.Add(interactor, CameraValues.Default);
        }

        public void UnregisterInteractor(object interactor) {
            if (!_data.ContainsKey(interactor)) return;

            _data.Remove(interactor);
        }

        public void AddPositionOffset(object interactor, Vector3 offsetDelta) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithPositionOffset(data.positionOffset + offsetDelta);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void SetPositionOffset(object interactor, Vector3 offset) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithPositionOffset(offset);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void ResetPositionOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithPositionOffset(Vector3.zero);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void Rotate(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithRotation(data.rotation * rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }

        public void SetRotation(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithRotation(rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void ResetRotation(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithRotation(Quaternion.identity);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithFovOffset(fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithFovOffset(data.fovOffset + fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _data[interactor];
            _data[interactor] = data.WithFovOffset(0f);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void InvalidateResultOffset() {
            var offset = Vector3.zero;
            foreach (var data in _data.Values) {
                offset += data.positionOffset;
            }
            _resultCameraValues = _resultCameraValues.WithPositionOffset(offset);
        }
        
        private void InvalidateResultRotation() {
            var rotation = Quaternion.identity;
            foreach (var data in _data.Values) {
                rotation *= data.rotation;
            }
            _resultCameraValues = _resultCameraValues.WithRotation(rotation);
        }
        
        private void InvalidateResultFovOffset() {
            float fovOffset = 0f;
            foreach (var data in _data.Values) {
                fovOffset += data.fovOffset;
            }
            _resultCameraValues = _resultCameraValues.WithFovOffset(fovOffset);
        }
        
        private void ApplyCameraParameters() {
            _transform.localPosition = _baseCameraValues.positionOffset + _resultCameraValues.positionOffset;
            _transform.localRotation = _baseCameraValues.rotation * _resultCameraValues.rotation;
            _camera.fieldOfView = _baseCameraValues.fovOffset + _resultCameraValues.fovOffset;
        }

        private bool CheckInteractorIsRegistered(object interactor) {
            if (_data.ContainsKey(interactor)) return true;

            Debug.LogWarning($"{nameof(CameraController)}: Not registered interactor {interactor} is trying to interact.");
            return false;
        }
    }

}
