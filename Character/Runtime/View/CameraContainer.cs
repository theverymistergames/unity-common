using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        public Camera Camera => _camera;
        
        private readonly Dictionary<int, CameraStateData> _states = new Dictionary<int, CameraStateData>();

        private CameraStateData _baseCameraState;
        private CameraStateData _resultCameraState;

        private float _invertedMaxWeight;
        private bool _isInitialized;
        private int _lastStateId;

        private void Awake() {
            _baseCameraState = new CameraStateData(
                weight: 1f,
                _transform.localPosition,
                _transform.localRotation,
                _camera.fieldOfView
            );

            _isInitialized = true;

            InvalidateResultState();
            ApplyResultState();
        }

        private void OnDestroy() {
            _states.Clear();
        }

        public int CreateState(float weight = 1f) {
            int id = _lastStateId++;
            _states[id] = new CameraStateData(weight);

            InvalidateWeights();
            InvalidateResultState();
            ApplyResultState();

            return id;
        }

        public void RemoveState(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            _states.Remove(id);
            
            InvalidateWeights();
            InvalidateResultState();
            ApplyResultState();
        }

        public void AddPositionOffset(int id, Vector3 offsetDelta) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithPosition(data.position + offsetDelta);
            
            InvalidateResultState(includeRotation: false, includeFov: false);
            ApplyResultState();
        }

        public void SetPositionOffset(int id, Vector3 offset) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithPosition(offset);
            
            InvalidateResultState(includeRotation: false, includeFov: false);
            ApplyResultState();
        }

        public void ResetPositionOffset(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithPosition(Vector3.zero);
            
            InvalidateResultState(includeRotation: false, includeFov: false);
            ApplyResultState();
        }

        public void AddRotationOffset(int id, Quaternion rotation) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithRotation(data.rotation * rotation);
            
            InvalidateResultState(includePosition: false, includeFov: false);
            ApplyResultState();
        }

        public void SetRotationOffset(int id, Quaternion rotation) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithRotation(rotation);
            
            InvalidateResultState(includePosition: false, includeFov: false);
            ApplyResultState();
        }
        
        public void ResetRotationOffset(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithRotation(Quaternion.identity);
            
            InvalidateResultState(includePosition: false, includeFov: false);
            ApplyResultState();
        }
        
        public void SetFovOffset(int id, float fov) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithFovOffset(fov);
            
            InvalidateResultState(includePosition: false, includeRotation: false);
            ApplyResultState();
        }
        
        public void AddFovOffset(int id, float fov) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithFovOffset(data.fov + fov);
            
            InvalidateResultState(includePosition: false, includeRotation: false);
            ApplyResultState();
        }
        
        public void ResetFovOffset(int id) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(id)) return;
#endif

            var data = _states[id];
            _states[id] = data.WithFovOffset(0f);
            
            InvalidateResultState(includePosition: false, includeRotation: false);
            ApplyResultState();
        }

        private void ApplyResultState() {
            if (!_isInitialized) return;

            _transform.localPosition = _baseCameraState.position + _resultCameraState.position;
            _transform.localRotation = _baseCameraState.rotation * _resultCameraState.rotation;
            _camera.fieldOfView = _baseCameraState.fov + _resultCameraState.fov;
        }

        private void InvalidateResultState(
            bool includePosition = true,
            bool includeRotation = true,
            bool includeFov = true
        ) {
            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            float fov = 0f;

            foreach (var data in _states.Values) {
                if (includePosition) {
                    position += data.weight * _invertedMaxWeight * data.position;
                }

                if (includeRotation) {
                    rotation *= Quaternion.SlerpUnclamped(Quaternion.identity, data.rotation, data.weight * _invertedMaxWeight);
                }

                if (includeFov) {
                    fov += data.weight * _invertedMaxWeight * data.fov;
                }
            }

            _resultCameraState = new CameraStateData(
                weight: 0f,
                includePosition ? position : _resultCameraState.position,
                includeRotation ? rotation : _resultCameraState.rotation,
                includeFov ? fov : _resultCameraState.fov
            );
        }

        private void InvalidateWeights() {
            float max = 0f;

            foreach (var data in _states.Values) {
                float absWeight = Mathf.Abs(data.weight);
                if (max < absWeight) max = absWeight;
            }

            _invertedMaxWeight = max <= 0f ? 0f : 1f / max;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool ValidateState(int id) {
            if (_states.ContainsKey(id)) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Not registered state #{id} is trying to interact.");
            return false;
        }
#endif
    }

}
