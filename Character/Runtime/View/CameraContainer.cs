using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour, IActorComponent, IUpdate {
        
        [SerializeField] private Transform _translationRoot;
        [SerializeField] private Transform _rotationRoot;
        [SerializeField] [Min(0f)] private float _positionSmoothing = 20f;
        [SerializeField] [Min(0f)] private float _rotationSmoothing = 30f;

        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }
        public bool EnableSmoothing { get; set; } = true;
        
        private readonly Dictionary<int, WeightedValue<Vector3>> _positionStates = new();
        private readonly Dictionary<int, WeightedValue<Quaternion>> _rotationStates = new();
        private readonly Dictionary<int, WeightedValue<float>> _fovStates = new();

        private ITimeSource _timeSource;

        private CameraState _baseState;
        private CameraState _resultState;
        private CameraState _persistentState;
        private CameraState _persistentStateBuffer;

        private Transform _cameraParent;
        private Vector3 _cameraOffset;
        private Quaternion _cameraRotationOffset;
        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;
        
        private bool _isInitialized;
        private int _lastStateId;
        private byte _clearPersistentStateOperationId;
        private bool _isClearingPersistentStates;

        void IActorComponent.OnAwake(IActor actor) {
            Camera = actor.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            
            CameraTransform.GetLocalPositionAndRotation(out _cameraOffset, out _cameraRotationOffset);
            CameraTransform.GetPositionAndRotation(out _cameraPosition, out _cameraRotation);
            _cameraParent = CameraTransform.parent;
            
            _timeSource = PlayerLoopStage.Update.Get();
            
            _baseState = new CameraState(_translationRoot.localPosition, _rotationRoot.localRotation, Camera.fieldOfView);
            _resultState = CameraState.Empty;
            _persistentState = CameraState.Empty;
            _persistentStateBuffer = CameraState.Empty;
            
            _isInitialized = true;
            
            ApplyResultState();
        }

        private void OnDestroy() {
            _isInitialized = false;
            
            _positionStates.Clear();
            _rotationStates.Clear();
            _fovStates.Clear();
        }

        private void OnEnable() {
            PlayerLoopStage.UnscaledUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.UnscaledUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            _cameraParent.GetPositionAndRotation(out var parentPos, out var parentRot);

            _cameraPosition = Vector3.Lerp(_cameraPosition, parentPos + parentRot * _cameraOffset, EnableSmoothing ? dt * _positionSmoothing : 1f);
            _cameraRotation = Quaternion.Slerp(_cameraRotation, parentRot * _cameraRotationOffset, EnableSmoothing ? dt * _rotationSmoothing : 1f);
            
            CameraTransform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        }

        public void PublishCameraPosition() {
            _cameraParent.GetPositionAndRotation(out var parentPos, out var parentRot);

            _cameraPosition = parentPos + parentRot * _cameraOffset;
            _cameraRotation = parentRot * _cameraRotationOffset;
            
            CameraTransform.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        }

        public int CreateState() {
            return _lastStateId++;
        }

        public void RemoveState(int id, bool keepChanges = false) {
            _positionStates.Remove(id);
            _rotationStates.Remove(id);
            _fovStates.Remove(id);
            
            var currentState = _resultState;
            _resultState = BuildResultState();
            
            if (keepChanges) SavePersistentState(currentState);
            
            ApplyResultState();
        }

        private void SavePersistentState(CameraState state) {
            ref var dest = ref _isClearingPersistentStates ? ref _persistentStateBuffer : ref _persistentState;
            dest = new CameraState(
                dest.position + state.position - _resultState.position,
                dest.rotation * state.rotation * Quaternion.Inverse(_resultState.rotation),
                dest.fov + state.fov - _resultState.fov
            );
        }

        public async UniTask ClearPersistentStates(float duration = 0f) {
            byte id = ++_clearPersistentStateOperationId;
            var cancellationToken = destroyCancellationToken;
            
            var startState = _persistentState;
            _persistentState = CameraState.Empty;
            _persistentStateBuffer = startState;

            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float t = 0f;
            _isClearingPersistentStates = true;
            
            while (id == _clearPersistentStateOperationId && !cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + speed * _timeSource.DeltaTime);

                _persistentStateBuffer = new CameraState(
                    Vector3.Lerp(_persistentStateBuffer.position, Vector3.zero, t),
                    Quaternion.Slerp(_persistentStateBuffer.rotation, Quaternion.identity, t),
                    Mathf.Lerp(_persistentStateBuffer.fov, 0f, t)
                );
                
                await UniTask.Yield();
                
                if (t >= 1f) break;
            }

            if (id != _clearPersistentStateOperationId || cancellationToken.IsCancellationRequested) return;
            
            _isClearingPersistentStates = false;
        }

        public void SetBasePositionOffset(Vector3 offset) {
            _baseState = _baseState.WithPosition(offset);
            ApplyResultState();
        }
        
        public void SetBaseRotationOffset(Quaternion offset) {
            _baseState = _baseState.WithRotation(offset);
            ApplyResultState();
        }
        
        public void SetBaseFov(float fov) {
            _baseState = _baseState.WithFov(fov);
            ApplyResultState();
        }

        public void AddPositionOffset(int id, float weight, Vector3 offsetDelta) {
            var data = _positionStates.GetValueOrDefault(id);
            _positionStates[id] = new WeightedValue<Vector3>(weight, data.value + offsetDelta);
            _resultState = _resultState.WithPosition(BuildResultPosition());
            
            ApplyResultState();
        }

        public void SetPositionOffset(int id, float weight, Vector3 offset) { 
            _positionStates[id] = new WeightedValue<Vector3>(weight, offset);
            _resultState = _resultState.WithPosition(BuildResultPosition());
            
            ApplyResultState();
        }

        public void ResetPositionOffset(int id, float weight) {
            _positionStates[id] = new WeightedValue<Vector3>(weight, Vector3.zero);
            _resultState = _resultState.WithPosition(BuildResultPosition());
            
            ApplyResultState();
        }

        public void AddRotationOffset(int id, float weight, Quaternion rotation) {
            var data = _rotationStates
                .GetValueOrDefault(id, new WeightedValue<Quaternion>(0f, Quaternion.identity));
            
            _rotationStates[id] = new WeightedValue<Quaternion>(weight, data.value * rotation);
            _resultState = _resultState.WithRotation(BuildResultRotation());
            
            ApplyResultState();
        }

        public void SetRotationOffset(int id, float weight, Quaternion rotation) {
            _rotationStates[id] = new WeightedValue<Quaternion>(weight, rotation);
            _resultState = _resultState.WithRotation(BuildResultRotation());
            
            ApplyResultState();
        }
        
        public void ResetRotationOffset(int id, float weight) {
            _rotationStates[id] = new WeightedValue<Quaternion>(weight, Quaternion.identity);
            _resultState = _resultState.WithRotation(BuildResultRotation());
            
            ApplyResultState();
        }
        
        public void AddFovOffset(int id, float weight, float fov) {
            var data = _fovStates.GetValueOrDefault(id);
            _fovStates[id] = new WeightedValue<float>(weight, data.value + fov);
            _resultState = _resultState.WithFov(BuildResultFov());
            
            ApplyResultState();
        }

        public void SetFovOffset(int id, float weight, float fov) {
            _fovStates[id] = new WeightedValue<float>(weight, fov);
            _resultState = _resultState.WithFov(BuildResultFov());
            
            ApplyResultState();
        }

        public void ResetFovOffset(int id, float weight) {
            _fovStates[id] = new WeightedValue<float>(weight, 0f);
            _resultState = _resultState.WithFov(BuildResultFov());
            
            ApplyResultState();
        }

        private void ApplyResultState() {
            if (!_isInitialized) return;

            _translationRoot.localPosition = _baseState.position + _persistentStateBuffer.position + _persistentState.position + _resultState.position;
            _rotationRoot.localRotation = _baseState.rotation * _persistentStateBuffer.rotation * _persistentState.rotation * _resultState.rotation;
            Camera.fieldOfView = _baseState.fov + _persistentStateBuffer.fov + _persistentState.fov + _resultState.fov;
        }

        private CameraState BuildResultState() {
            return new CameraState(
                BuildResultPosition(),
                BuildResultRotation(),
                BuildResultFov()
            );
        }
        
        private Vector3 BuildResultPosition() {
            var result = Vector3.zero;
            float invertedMaxWeight = BuildInvertedMaxWeight(_positionStates);
            
            foreach (var data in _positionStates.Values) {
                result += data.weight * invertedMaxWeight * data.value;
            }
            
            return result;
        }

        private Quaternion BuildResultRotation() {
            var result = Quaternion.identity;
            float invertedMaxWeight = BuildInvertedMaxWeight(_rotationStates);
            
            foreach (var data in _rotationStates.Values) {
                result *= Quaternion.SlerpUnclamped(Quaternion.identity, data.value, data.weight * invertedMaxWeight);
            }
            
            return result;
        }
        
        private float BuildResultFov() {
            float result = 0f;
            float invertedMaxWeight = BuildInvertedMaxWeight(_fovStates);
            
            foreach (var data in _fovStates.Values) {
                result += data.weight * invertedMaxWeight * data.value;
            }
            
            return result;
        }
        
        private static float BuildInvertedMaxWeight<T>(Dictionary<int, WeightedValue<T>> source) {
            float maxWeight = 0f;
            
            foreach (var data in source.Values) {
                float absWeight = Mathf.Abs(data.weight);
                if (maxWeight < absWeight) maxWeight = absWeight;
            }
            
            return maxWeight <= 0f ? 0f : 1f / maxWeight;
        }
    }

}
