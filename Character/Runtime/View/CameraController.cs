using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CameraController : MonoBehaviour {

        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;
        public Quaternion Rotation => _cameraTransform.rotation;

        private Transform _cameraTransform;
        private Vector3 _cameraBaseOffset;
        private Quaternion _cameraBaseRotation;
        private float _cameraBaseFov = 0f;
        
        private Vector3 _cameraResultOffset = Vector3.zero;
        private Quaternion _cameraResultRotation = Quaternion.identity;
        private float _cameraResultFovOffset = 0f;
        
        private readonly Dictionary<Object, CameraInteractorData> _interactors = new Dictionary<Object, CameraInteractorData>();

        private struct CameraInteractorData {

            public Vector3 offset;
            public Quaternion rotation;
            public float fovOffset;

            public static readonly CameraInteractorData Default = new CameraInteractorData {
                offset = Vector3.zero,
                rotation = Quaternion.identity,
                fovOffset = 0f
            };
        }

        private void Awake() {
            _cameraTransform = _camera.transform;
            _cameraBaseOffset = _cameraTransform.localPosition;
            _cameraBaseRotation = _cameraTransform.localRotation;
            _cameraBaseFov = _camera.fieldOfView;
        }

        private void OnDestroy() {
            _interactors.Clear();
        }

        public void RegisterInteractor(Object interactor) {
            if (_interactors.ContainsKey(interactor)) return;

            _interactors.Add(interactor, CameraInteractorData.Default);
        }

        public void UnregisterInteractor(Object interactor) {
            if (!_interactors.ContainsKey(interactor)) return;

            _interactors.Remove(interactor);
        }

        public void AddOffset(Object interactor, Vector3 motion) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.offset += motion;
            _interactors[interactor] = data;
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void SetOffset(Object interactor, Vector3 offset) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.offset = offset;
            _interactors[interactor] = data;
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void ResetOffset(Object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.offset = Vector3.zero;
            _interactors[interactor] = data;
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void Rotate(Object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.rotation *= rotation;
            _interactors[interactor] = data;
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }

        public void SetRotation(Object interactor, Quaternion rotation) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.rotation = rotation;
            _interactors[interactor] = data;
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void ResetRotation(Object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.rotation = Quaternion.identity;
            _interactors[interactor] = data;
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(Object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.fovOffset = fov;
            _interactors[interactor] = data;
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(Object interactor, float fov) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.fovOffset += fov;
            _interactors[interactor] = data;
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(Object interactor) {
            if (!CheckInteractorIsRegistered(interactor)) return;

            var data = _interactors[interactor];
            data.fovOffset = 0f;
            _interactors[interactor] = data;
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void InvalidateResultOffset() {
            _cameraResultOffset = Vector3.zero;
            foreach (var data in _interactors.Values) {
                _cameraResultOffset += data.offset;
            }
        }
        
        private void InvalidateResultRotation() {
            _cameraResultRotation = Quaternion.identity;
            foreach (var data in _interactors.Values) {
                _cameraResultRotation *= data.rotation;
            }
        }
        
        private void InvalidateResultFovOffset() {
            _cameraResultFovOffset = 0f;
            foreach (var data in _interactors.Values) {
                _cameraResultFovOffset += data.fovOffset;
            }
        }
        
        private void ApplyCameraParameters() {
            _cameraTransform.localPosition = _cameraBaseOffset + _cameraResultOffset;
            _cameraTransform.localRotation = _cameraBaseRotation * _cameraResultRotation;
            _camera.fieldOfView = _cameraBaseFov + _cameraResultFovOffset;
        }

        private bool CheckInteractorIsRegistered(Object interactor) {
            if (_interactors.ContainsKey(interactor)) return true;

            Debug.LogWarning($"{nameof(CameraController)}: Not registered interactor {interactor} is trying to interact.");
            return false;
        }
    }

}
