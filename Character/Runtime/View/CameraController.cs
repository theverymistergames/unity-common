using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CameraController : MonoBehaviour {

        [SerializeField] private Camera _camera;

        public Quaternion Rotation => _cameraTransform.rotation;
        
        private Transform _cameraTransform;
        private Vector3 _cameraBaseOffset;
        private Quaternion _cameraBaseRotation;
        private float _cameraBaseFov = 0f;
        
        private Vector3 _cameraResultOffset = Vector3.zero;
        private Quaternion _cameraResultRotation = Quaternion.identity;
        private float _cameraResultFovOffset = 0f;
        
        private readonly ObjectDataMap<CameraInteractorData> _interactors = new ObjectDataMap<CameraInteractorData>();
        
        private void Awake() {
            _cameraTransform = _camera.transform;
            _cameraBaseOffset = _cameraTransform.localPosition;
            _cameraBaseRotation = _cameraTransform.localRotation;
            _cameraBaseFov = _camera.fieldOfView;
        }

        private void OnDestroy() {
            _interactors.Clear();
        }

        public void RegisterInteractor(Object source) {
            _interactors.Register(source, CameraInteractorData.Default);
        }

        public void UnregisterInteractor(Object source) {
            _interactors.Unregister(source);
        }

        public void AddOffset(Object source, Vector3 motion) {
            var data = _interactors.Get(source);
            data.offset += motion;
            _interactors.Set(source, data);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void SetOffset(Object source, Vector3 offset) {
            var data = _interactors.Get(source);
            data.offset = offset;
            _interactors.Set(source, data);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void ResetOffset(Object source) {
            var data = _interactors.Get(source);
            data.offset = Vector3.zero;
            _interactors.Set(source, data);
            
            InvalidateResultOffset();
            ApplyCameraParameters();
        }

        public void Rotate(Object source, Quaternion rotation) {
            var data = _interactors.Get(source);
            data.rotation *= rotation;
            _interactors.Set(source, data);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }

        public void SetRotation(Object source, Quaternion rotation) {
            var data = _interactors.Get(source);
            data.rotation = rotation;
            _interactors.Set(source, data);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void ResetRotation(Object source) {
            var data = _interactors.Get(source);
            data.rotation = Quaternion.identity;
            _interactors.Set(source, data);
            
            InvalidateResultRotation();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(Object source, float fov) {
            var data = _interactors.Get(source);
            data.fovOffset = fov;
            _interactors.Set(source, data);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(Object source, float fov) {
            var data = _interactors.Get(source);
            data.fovOffset += fov;
            _interactors.Set(source, data);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(Object source) {
            var data = _interactors.Get(source);
            data.fovOffset = 0f;
            _interactors.Set(source, data);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void InvalidateResultOffset() {
            _cameraResultOffset = Vector3.zero;
            for (int i = 0; i < _interactors.Count; i++) {
                _cameraResultOffset += _interactors[i].offset;
            }
        }
        
        private void InvalidateResultRotation() {
            _cameraResultRotation = Quaternion.identity;
            for (int i = 0; i < _interactors.Count; i++) {
                _cameraResultRotation *= _interactors[i].rotation;
            }
        }
        
        private void InvalidateResultFovOffset() {
            _cameraResultFovOffset = 0f;
            for (int i = 0; i < _interactors.Count; i++) {
                _cameraResultFovOffset += _interactors[i].fovOffset;
            }
        }
        
        private void ApplyCameraParameters() {
            _cameraTransform.localPosition = _cameraBaseOffset + _cameraResultOffset;
            _cameraTransform.localRotation = _cameraBaseRotation * _cameraResultRotation;
            _camera.fieldOfView = _cameraBaseFov + _cameraResultFovOffset;
        }

        private struct CameraInteractorData {
            
            public static readonly CameraInteractorData Default = new CameraInteractorData {
                offset = Vector3.zero,
                rotation = Quaternion.identity,
                fovOffset = 0f
            };
            
            public Vector3 offset;
            public Quaternion rotation;
            public float fovOffset;
            
        }
        
    }

}
