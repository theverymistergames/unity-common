using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        private readonly Dictionary<int, CameraStateKey> _keyMap = new Dictionary<int, CameraStateKey>();
        private readonly List<CameraState> _states = new List<CameraState>();

        private CameraState _baseCameraState;
        private CameraState _resultCameraState;
        private float _invertedMaxWeight;

        private void Awake() {
            _baseCameraState = new CameraState(
                hash: 0,
                weight: 1f,
                _transform.localPosition,
                _transform.localRotation,
                _camera.fieldOfView
            );
        }

        private void OnDestroy() {
            _states.Clear();
            _keyMap.Clear();
        }

        public CameraStateKey CreateState(object source, float weight = 1f) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateStateSource(source)) return default;
#endif

            int hash = source.GetHashCode();
            var key = new CameraStateKey(hash, 0, _states.Count);

            if (_keyMap.TryGetValue(hash, out var existentKey)) {
                _states[existentKey.index] = _states[existentKey.index].WithWeight(weight);
                key = new CameraStateKey(hash, existentKey.token + 1, existentKey.index);
            }
            else {
                _states.Add(new CameraState(hash, weight));
            }

            _keyMap[hash] = key;

            InvalidateInvertedMaxWeight();
            InvalidateResultPositionOffset();
            InvalidateResultRotationOffset();
            InvalidateResultFovOffset();

            ApplyCameraParameters();

            return key;
        }

        public void RemoveState(CameraStateKey key, bool keepChanges = false) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            int lastToken = _keyMap[key.hash].token;
            if (key.token != lastToken) return;

            var state = _states[key.index];
            _states[key.index] = state.WithWeight(0f);

            _keyMap.Remove(key.hash);

            if (keepChanges) {
                float w = state.weight * _invertedMaxWeight;
                _baseCameraState = new CameraState(
                    _baseCameraState.hash,
                    _baseCameraState.weight,
                    _baseCameraState.position + w * state.position,
                    _baseCameraState.rotation * Quaternion.SlerpUnclamped(Quaternion.identity, state.rotation, w),
                    _baseCameraState.fov + w * state.fov
                );
            }

            for (int i = _states.Count - 1; i >= 0; i--) {
                int hash = _states[i].hash;
                if (_keyMap.ContainsKey(hash)) break;

                _states.RemoveAt(i);
                _keyMap.Remove(hash);
            }

            InvalidateInvertedMaxWeight();
            InvalidateResultPositionOffset();
            InvalidateResultRotationOffset();
            InvalidateResultFovOffset();

            ApplyCameraParameters();
        }

        public void AddPositionOffset(CameraStateKey key, Vector3 offsetDelta) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithPosition(data.position + offsetDelta);
            
            InvalidateResultPositionOffset();
            ApplyCameraParameters();
        }

        public void SetPositionOffset(CameraStateKey key, Vector3 offset) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithPosition(offset);
            
            InvalidateResultPositionOffset();
            ApplyCameraParameters();
        }

        public void ResetPositionOffset(CameraStateKey key) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithPosition(Vector3.zero);
            
            InvalidateResultPositionOffset();
            ApplyCameraParameters();
        }

        public void AddRotationOffset(CameraStateKey key, Quaternion rotation) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithRotation(data.rotation * rotation);
            
            InvalidateResultRotationOffset();
            ApplyCameraParameters();
        }

        public void SetRotationOffset(CameraStateKey key, Quaternion rotation) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithRotation(rotation);
            
            InvalidateResultRotationOffset();
            ApplyCameraParameters();
        }
        
        public void ResetRotationOffset(CameraStateKey key) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithRotation(Quaternion.identity);
            
            InvalidateResultRotationOffset();
            ApplyCameraParameters();
        }
        
        public void SetFovOffset(CameraStateKey key, float fov) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithFovOffset(fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void AddFovOffset(CameraStateKey key, float fov) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithFovOffset(data.fov + fov);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }
        
        public void ResetFovOffset(CameraStateKey key) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            var data = _states[key.index];
            _states[key.index] = data.WithFovOffset(0f);
            
            InvalidateResultFovOffset();
            ApplyCameraParameters();
        }

        private void ApplyCameraParameters() {
            _transform.localPosition = _baseCameraState.position + _resultCameraState.position;
            _transform.localRotation = _baseCameraState.rotation * _resultCameraState.rotation;
            _camera.fieldOfView = _baseCameraState.fov + _resultCameraState.fov;
        }

        private void InvalidateResultPositionOffset() {
            var position = Vector3.zero;

            for (int i = 0; i < _states.Count; i++) {
                var data = _states[i];
                position += data.weight * _invertedMaxWeight * data.position;
            }

            _resultCameraState = _resultCameraState.WithPosition(position);
        }

        private void InvalidateResultRotationOffset() {
            var rotation = Quaternion.identity;

            for (int i = 0; i < _states.Count; i++) {
                var data = _states[i];
                rotation *= Quaternion.SlerpUnclamped(Quaternion.identity, data.rotation, data.weight * _invertedMaxWeight);
            }

            _resultCameraState = _resultCameraState.WithRotation(rotation);
        }

        private void InvalidateResultFovOffset() {
            float fov = 0f;

            for (int i = 0; i < _states.Count; i++) {
                var data = _states[i];
                fov += data.weight * _invertedMaxWeight * data.fov;
            }

            _resultCameraState = _resultCameraState.WithFovOffset(fov);
        }

        private void InvalidateInvertedMaxWeight() {
            float max = 0f;

            for (int i = 0; i < _states.Count; i++) {
                float absWeight = Mathf.Abs(_states[i].weight);
                if (max < absWeight) max = absWeight;
            }

            _invertedMaxWeight = max <= 0f ? 0f : 1f / max;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool ValidateStateSource(object source) {
            if (source != null) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Cannot create state for null source object.");
            return false;
        }

        private bool ValidateState(CameraStateKey key) {
            if (_keyMap.TryGetValue(key.hash, out var existentKey) && key.index == existentKey.index) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Not registered state #{key.index} is trying to interact.");
            return false;
        }
#endif
    }

}
