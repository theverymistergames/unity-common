using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _transform;

        private readonly Dictionary<int, int> _stateHashToIndexMap = new Dictionary<int, int>();
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
            _stateHashToIndexMap.Clear();
        }

        public CameraStateKey CreateState(object source, float weight = 1f) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateStateSource(source)) return default;
#endif

            int hash = source.GetHashCode();
            int index = _states.Count;

            if (_stateHashToIndexMap.TryGetValue(hash, out int existentIndex)) {
                index = existentIndex;
                _states[index] = _states[index].WithWeight(weight);
            }
            else {
                _states.Add(new CameraState(hash, weight));
                _stateHashToIndexMap[hash] = index;
            }

            InvalidateInvertedMaxWeight();

            return new CameraStateKey(index, hash);
        }

        public void RemoveState(CameraStateKey key) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!ValidateState(key)) return;
#endif

            _states[key.index] = _states[key.index].WithWeight(0f);
            _stateHashToIndexMap.Remove(key.hash);

            for (int i = _states.Count - 1; i >= 0; i--) {
                int hash = _states[i].hash;
                if (_stateHashToIndexMap.ContainsKey(hash)) break;

                _states.RemoveAt(i);
                _stateHashToIndexMap.Remove(hash);
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
                rotation *= Quaternion.Slerp(Quaternion.identity, data.rotation, data.weight * _invertedMaxWeight);
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
                var data = _states[i];
                if (max < data.weight) max = data.weight;
            }

            _invertedMaxWeight = max <= 0f ? 0f : 1f / max;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool ValidateStateSource(object source) {
            if (source != null) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Cannot create interactor for null object.");
            return false;
        }

        private bool ValidateState(CameraStateKey key) {
            if (_stateHashToIndexMap.TryGetValue(key.hash, out int index) && key.index == index) return true;

            Debug.LogWarning($"{nameof(CameraContainer)}: Not registered state {key} is trying to interact.");
            return false;
        }
#endif
    }

}
