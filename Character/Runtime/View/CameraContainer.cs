using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        public Vector3 Position => _transform.position;
        public Quaternion Rotation => _transform.rotation;

        private CameraParameters _baseCameraParameters;
        private CameraParameters _resultCameraParameters;

        private readonly Dictionary<object, CameraParameters> _cameraParametersMap = new Dictionary<object, CameraParameters>();

        private void Awake() {
            _baseCameraParameters = new CameraParameters(_transform.localPosition, _transform.localRotation, _camera.fieldOfView);
        }

        private void OnDestroy() {
            _cameraParametersMap.Clear();
        }

        public void RegisterInteractor(object interactor) {
            if (_cameraParametersMap.ContainsKey(interactor)) return;

            _cameraParametersMap.Add(interactor, CameraParameters.Default);
        }

        public void UnregisterInteractor(object interactor) {
            if (!_cameraParametersMap.ContainsKey(interactor)) return;

            _cameraParametersMap.Remove(interactor);

            InvalidateResultOffset();
            InvalidateResultRotation();
            InvalidateResultFovOffset();

            ApplyCameraParameters();
        }

        public void AddPositionOffset(object interactor, Vector3 offsetDelta) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithPosition(data.position + offsetDelta);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void SetPositionOffset(object interactor, Vector3 offset) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithPosition(offset);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void ResetPositionOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithPosition(Vector3.zero);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void AddRotationOffset(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithRotation(data.rotation * rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }

        public void SetRotationOffset(object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithRotation(rotation);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void ResetRotationOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithRotation(Quaternion.identity);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithFovOffset(fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithFovOffset(data.fov + fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _cameraParametersMap[interactor];
            _cameraParametersMap[interactor] = data.WithFovOffset(0f);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void InvalidateResultOffset() {
            var position = Vector3.zero;
            foreach (var data in _cameraParametersMap.Values) {
                position += data.position;
            }

            _resultCameraParameters = _resultCameraParameters.WithPosition(position);
        }
        
        private void InvalidateResultRotation() {
            var rotation = Quaternion.identity;
            foreach (var data in _cameraParametersMap.Values) {
                rotation *= data.rotation;
            }

            _resultCameraParameters = _resultCameraParameters.WithRotation(rotation);
        }
        
        private void InvalidateResultFovOffset() {
            float fov = 0f;
            foreach (var data in _cameraParametersMap.Values) {
                fov += data.fov;
            }

            _resultCameraParameters = _resultCameraParameters.WithFovOffset(fov);
        }
        
        private void ApplyCameraParameters() {
            _transform.localPosition = _baseCameraParameters.position + _resultCameraParameters.position;
            _transform.localRotation = _baseCameraParameters.rotation * _resultCameraParameters.rotation;
            _camera.fieldOfView = _baseCameraParameters.fov + _resultCameraParameters.fov;
        }

        private bool CheckInteractorIsRegistered(object interactor) {
            if (_cameraParametersMap.ContainsKey(interactor)) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Not registered interactor {interactor} is trying to interact.");
            return false;
        }
    }

}
