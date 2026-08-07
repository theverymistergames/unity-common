using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Common.Async;
using MisterGames.Common.Maths;
using MisterGames.Common.Stats;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour, IActorComponent {
        
        [Header("Transforms")]
        [SerializeField] private Transform _translationRoot;
        [SerializeField] private Transform _rotationRoot;
        
        [Header("Timescale")]
        [SerializeField] private bool _useUnscaledTime;
        
        public enum MaskMode {
            And,
            Xand,
            Or,
            Xor,
        }

        public enum FovMode {
            AdditiveOffset,
            AverageOffset,
            LowerLimit,
            UpperLimit,
            AbsoluteValue,
        }

        private const float WeightTolerance = 0.00001f;
        
        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }
        
        private readonly Dictionary<int, WeightedValue<Vector3>> _positionStates = new();
        private readonly Dictionary<int, WeightedValue<Quaternion>> _rotationStates = new();
        private readonly Dictionary<int, WeightedValue<(FovMode mode, float value)>> _fovStates = new();
        private readonly Dictionary<int, (int mask, MaskMode mode)> _cullingMaskStates = new();

        private CancellationTokenSource _destroyCts;
        private CameraState _baseState;
        private CameraState _resultState;
        private CameraState _persistentState;
        private CameraState _persistentStateBuffer;
        private int _defaultCullingMask;
        private int _resultCullingMask;

        private Vector3 _cameraOffset;
        private Quaternion _cameraRotationOffset;
        
        private bool _isInitialized;
        private int _lastStateId;
        private byte _clearPersistentStateOperationId;
        private bool _isClearingPersistentStates;

        void IActorComponent.OnAwake(IActor actor) {
            AsyncExt.RecreateCts(ref _destroyCts);
            
            Camera = actor.GetComponent<Camera>();
            CameraTransform = Camera.transform;
            
            _baseState = new CameraState(_translationRoot.localPosition, _rotationRoot.localRotation, Camera.fieldOfView);
            _resultState = CameraState.Empty;
            _persistentState = CameraState.Empty;
            _persistentStateBuffer = CameraState.Empty;

            _defaultCullingMask = Camera.cullingMask;
            _resultCullingMask = _defaultCullingMask;
            
            _isInitialized = true;
            
            ApplyResultState();
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            _isInitialized = false;
            
            _positionStates.Clear();
            _rotationStates.Clear();
            _fovStates.Clear();
        }

#if UNITY_EDITOR
        public int CreateState(
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = "", 
            [CallerLineNumber] int lineNumber = 0) 
        {
#else
        public int CreateState() {
#endif      
            int id = _lastStateId.IncrementUncheckedRef();

#if UNITY_EDITOR
            if (_showDebugInfo) Log($"created state {id} for {filePath}, {memberName}, line {lineNumber}, state: {GetStateAsString()}");
#endif
            
            return id;
        }

        public void RemoveState(int id, bool keepChanges = false) {
            _positionStates.Remove(id);
            _rotationStates.Remove(id);
            _fovStates.Remove(id);

#if UNITY_EDITOR
            if (_showDebugInfo) Log($"remove state {id}, keepChanges: {keepChanges}, state: {GetStateAsString()}");
#endif
            
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
        
        private float GetDeltaTime() {
            return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        public async UniTask ClearPersistentStates(float duration = 0f, CancellationToken cancellationToken = default) {
            byte id = _clearPersistentStateOperationId.IncrementUncheckedRef();
            var destroyToken = _destroyCts.Token;
            
#if UNITY_EDITOR
            if (_showDebugInfo) Log($"clearing persistent state in {duration:0.000} sec, operation id {id}, state: {GetStateAsString()}");
#endif
            
            var startState = _persistentState;
            _persistentState = CameraState.Empty;
            _persistentStateBuffer = startState;

            float speed = duration > 0f ? 1f / duration : float.MaxValue;
            float t = 0f;
            _isClearingPersistentStates = true;
            
            while (id == _clearPersistentStateOperationId && 
                   !cancellationToken.IsCancellationRequested && 
                   !destroyToken.IsCancellationRequested) 
            {
                t = Mathf.Clamp01(t + speed * GetDeltaTime());

                _persistentStateBuffer = new CameraState(
                    Vector3.Lerp(_persistentStateBuffer.position, Vector3.zero, t),
                    Quaternion.Slerp(_persistentStateBuffer.rotation, Quaternion.identity, t),
                    Mathf.Lerp(_persistentStateBuffer.fov, 0f, t)
                );
                
                // Avoid waiting one frame if operation is done on the frame it started.
                if (t >= 1f) break;
                
                await UniTask.Yield();
            }

            if (id != _clearPersistentStateOperationId || 
                destroyToken.IsCancellationRequested) 
            {
                return;
            }
            
            _isClearingPersistentStates = false;
            
#if UNITY_EDITOR
            if (_showDebugInfo) Log($"finished clearing persistent state, operation id {id}, state: {GetStateAsString()}");
#endif
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
            if (_baseState.fov.IsNearlyEqual(fov)) return;
            
            _baseState = _baseState.WithFov(fov);
            _resultState = _resultState.WithFov(BuildResultFovOffset(_baseState.fov));
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

        public void SetFov(int id, float weight, float fov, FovMode mode) {
            _fovStates[id] = new WeightedValue<(FovMode, float)>(weight, (mode, fov));
            _resultState = _resultState.WithFov(BuildResultFovOffset(_baseState.fov));
            
            ApplyResultState();
        }

        public void ResetFov(int id, float weight) {
            _fovStates[id] = new WeightedValue<(FovMode, float)>(weight, (FovMode.AdditiveOffset, 0f));
            _resultState = _resultState.WithFov(BuildResultFovOffset(_baseState.fov));

            ApplyResultState();
        }

        public void SetCullingMask(int id, int mask, MaskMode mode = MaskMode.And) {
            _cullingMaskStates[id] = (mask, mode);
            _resultCullingMask = BuildResultCullingMask();
            
            ApplyCullingMask();
        }

        public void RemoveCullingMask(int id) {
            _cullingMaskStates.Remove(id);
            _resultCullingMask = BuildResultCullingMask();
            
            ApplyCullingMask();
        }

        private void ApplyResultState() {
            if (!_isInitialized) return;

            _translationRoot.localPosition = _baseState.position + _persistentStateBuffer.position + _persistentState.position + _resultState.position;
            _rotationRoot.localRotation = _baseState.rotation * _persistentStateBuffer.rotation * _persistentState.rotation * _resultState.rotation;
            Camera.fieldOfView = _baseState.fov + _persistentStateBuffer.fov + _persistentState.fov + _resultState.fov;
        }

        private void ApplyCullingMask() {
            Camera.cullingMask = _resultCullingMask;
        }

        private int BuildResultCullingMask() {
            int mask = _defaultCullingMask;
            
            foreach (var data in _cullingMaskStates.Values) {
                mask = data.mode switch {
                    MaskMode.And => mask & data.mask,
                    MaskMode.Xand => mask & ~data.mask,
                    MaskMode.Or => mask | data.mask,
                    MaskMode.Xor => mask ^ data.mask,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return mask;
        }
        
        private CameraState BuildResultState() {
            return new CameraState(BuildResultPosition(), BuildResultRotation(), BuildResultFovOffset(_baseState.fov));
        }
        
        private Vector3 BuildResultPosition() {
            var result = Vector3.zero;
            float w = BuildInvertedMaxWeight(_positionStates);
            
            foreach (var data in _positionStates.Values) {
                result += w * data.weight * data.value;
            }
            
            return result;
        }

        private Quaternion BuildResultRotation() {
            var result = Quaternion.identity;
            float w = BuildInvertedMaxWeight(_rotationStates);
            
            foreach (var data in _rotationStates.Values) {
                result *= Quaternion.SlerpUnclamped(Quaternion.identity, data.value, data.weight * w);
            }
            
            return result;
        }
        
        private float BuildResultFovOffset(float baseFov) {
            float accumAddWeightMax = 0f;
            float accumAvgWeightSum = 0f;
            float lowerBoundWeightSum = 0f;
            float upperBoundWeightSum = 0f;
            float setWeightSum = 0f;
            
            foreach (var data in _fovStates.Values) {
                float wAbs = Mathf.Abs(data.weight);
                
                switch (data.value.mode) {
                    case FovMode.AdditiveOffset:
                        if (wAbs > accumAddWeightMax) accumAddWeightMax = wAbs;
                        break;
                    case FovMode.AverageOffset:
                        accumAvgWeightSum += wAbs;
                        break;
                    case FovMode.LowerLimit:
                        lowerBoundWeightSum += wAbs;
                        break;
                    case FovMode.UpperLimit:
                        upperBoundWeightSum += wAbs;
                        break;
                    case FovMode.AbsoluteValue:
                        setWeightSum += wAbs;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            float accumAddWeightMaxInv = accumAddWeightMax >= WeightTolerance ? 1f / accumAddWeightMax : 0f;
            float accumAvgWeightSumInv = accumAvgWeightSum >= WeightTolerance ? 1f / accumAvgWeightSum : 0f;
            float lowerBoundWeightSumInv = lowerBoundWeightSum >= WeightTolerance ? 1f / lowerBoundWeightSum : 0f;
            float upperBoundWeightSumInv = upperBoundWeightSum >= WeightTolerance ? 1f / upperBoundWeightSum : 0f;
            float setWeightSumInv = setWeightSum >= WeightTolerance ? 1f / setWeightSum : 0f;

            float accumAdd = 0f;
            float accumAvg = 0f;
            float accumLowerBound = 0f;
            float accumUpperBound = 0f;
            float accumSet = 0f;
            
            foreach (var data in _fovStates.Values) {
                var modifier = data.value;
                
                switch (data.value.mode) {
                    case FovMode.AdditiveOffset:
                        accumAdd += Mathf.LerpUnclamped(0f, modifier.value, data.weight * accumAddWeightMaxInv);
                        break;
                    case FovMode.AverageOffset:
                        accumAvg += modifier.value * data.weight * accumAvgWeightSumInv;
                        break;
                    case FovMode.LowerLimit:
                        accumLowerBound += modifier.value * data.weight * lowerBoundWeightSumInv;
                        break;
                    case FovMode.UpperLimit:
                        accumUpperBound += modifier.value * data.weight * upperBoundWeightSumInv;
                        break;
                    case FovMode.AbsoluteValue:
                        accumSet += modifier.value * data.weight * setWeightSumInv;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            float lowerBound = lowerBoundWeightSum > 0f ? accumLowerBound : float.MinValue;
            float upperBound = upperBoundWeightSum > 0f ? accumUpperBound : float.MaxValue;

            float fov = setWeightSum > 0f 
                ? accumSet
                : Mathf.Clamp(baseFov + accumAvg + accumAdd, lowerBound, upperBound);
            
            return fov - baseFov;
        }
        
        private static float BuildInvertedMaxWeight<T>(Dictionary<int, WeightedValue<T>> source) {
            float max = 0f;
            
            foreach (var data in source.Values) {
                float w = Mathf.Abs(data.weight);
                if (w > max) max = w;
            }
            
            return max <= WeightTolerance ? 0f : 1f / max;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;

        private void Log(string message) {
            Debug.Log($"{nameof(CameraContainer).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
        
        private string GetStateAsString() {
            var sb = new StringBuilder();

            sb.AppendLine($"Position states ({_positionStates.Count}):");
            foreach ((int id, var data) in _positionStates) {
                sb.AppendLine($"[{id}] w {data.weight:0.00}, value {data.value}");
            }
            
            sb.AppendLine($"Rotation states ({_rotationStates.Count}):");
            foreach ((int id, var data) in _rotationStates) {
                sb.AppendLine($"[{id}] w {data.weight:0.00}, value {data.value}");
            }
            
            sb.AppendLine($"Fov add states ({_fovStates.Count}):");
            foreach ((int id, var data) in _fovStates) {
                sb.AppendLine($"[{id}] w {data.weight:0.00}, value {data.value}");
            }
            return sb.ToString();
        }
#endif
    }

}
