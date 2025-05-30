﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CameraContainer : MonoBehaviour, IActorComponent {
        
        [SerializeField] private Transform _translationRoot;
        [SerializeField] private Transform _rotationRoot;

        public enum MaskMode {
            And,
            Xand,
            Or,
            Xor,
        }
        
        private const float WeightTolerance = 0.00001f;
        
        public Camera Camera { get; private set; }
        public Transform CameraTransform { get; private set; }
        
        private readonly Dictionary<int, WeightedValue<Vector3>> _positionStates = new();
        private readonly Dictionary<int, WeightedValue<Quaternion>> _rotationStates = new();
        private readonly Dictionary<int, WeightedValue<float>> _fovStates = new();
        private readonly Dictionary<int, (int mask, MaskMode mode)> _cullingMaskStates = new();

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
            _isInitialized = false;
            
            _positionStates.Clear();
            _rotationStates.Clear();
            _fovStates.Clear();
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
                t = Mathf.Clamp01(t + speed * Time.deltaTime);

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
            return new CameraState(BuildResultPosition(), BuildResultRotation(), BuildResultFov());
        }
        
        private Vector3 BuildResultPosition() {
            var result = Vector3.zero;
            float w = BuildWeightMultiplier(_positionStates);
            
            foreach (var data in _positionStates.Values) {
                result += w * data.weight * data.value;
            }
            
            return result;
        }

        private Quaternion BuildResultRotation() {
            var result = Quaternion.identity;
            float w = BuildWeightMultiplier(_rotationStates);
            
            foreach (var data in _rotationStates.Values) {
                result *= Quaternion.SlerpUnclamped(Quaternion.identity, data.value, data.weight * w);
            }
            
            return result;
        }
        
        private float BuildResultFov() {
            float result = 0f;
            float w = BuildWeightMultiplier(_fovStates);
            
            foreach (var data in _fovStates.Values) {
                result += w * data.weight * data.value;
            }
            
            return result;
        }
        
        private static float BuildWeightMultiplier<T>(Dictionary<int, WeightedValue<T>> source) {
            float max = 0f;
            
            foreach (var data in source.Values) {
                float w = Mathf.Abs(data.weight);
                if (w > max) max = w;
            }
            
            return max <= WeightTolerance ? 0f : 1f / max;
        }
    }

}
