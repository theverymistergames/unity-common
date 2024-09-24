using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CameraShaker : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Test")]
        [SerializeField] private float _weight;
        [SerializeField] private float _noiseScale;
        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private Vector3 _positionMultiplier;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private Vector3 _rotationMultiplier;
        
        private readonly Dictionary<int, float> _weightMap = new();
        private readonly Dictionary<int, float> _noiseScaleMap = new();
        private readonly Dictionary<int, ShakeData> _positionMap = new();
        private readonly Dictionary<int, ShakeData> _rotationMap = new();
        
        private CameraContainer _cameraContainer;

        private readonly struct ShakeData {
            public readonly Vector3 offset;
            public readonly Vector3 multiplier;
            
            public ShakeData(Vector3 offset, Vector3 multiplier) {
                this.offset = offset;
                this.multiplier = multiplier;
            }
        }
        
        void IActorComponent.OnAwake(IActor actor) {
            _cameraContainer = actor.GetComponent<CameraContainer>();
        }

        private void OnEnable() {
            if (_weightMap.Count > 0) PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public int CreateState(float weight) {
            int id = _cameraContainer.CreateState();
            
            _weightMap[id] = weight;
            _noiseScaleMap[id] = 0f;
            _positionMap[id] = default;
            _rotationMap[id] = default;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            return id;
        }

        public void RemoveState(int id) {
            _cameraContainer.RemoveState(id);
            
            _weightMap.Remove(id);
            _noiseScaleMap.Remove(id);
            _positionMap.Remove(id);
            _rotationMap.Remove(id);
            
            if (_weightMap.Count == 0) PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public void SetNoiseScale(int id, float noiseScale) {
            _noiseScaleMap[id] = noiseScale;
        }

        public void SetPosition(int id, Vector3 offset, Vector3 multiplier) {
            _positionMap[id] = new ShakeData(offset, multiplier);
        }
        
        public void SetRotation(int id, Vector3 offset, Vector3 multiplier) {
            _rotationMap[id] = new ShakeData(offset, multiplier);
        }

        void IUpdate.OnUpdate(float dt) {
            float time = Time.time;
            
            foreach ((int id, float w) in _weightMap) {
                float s = _noiseScaleMap[id] * time;
                var position = _positionMap[id];
                var rotation = _rotationMap[id];
                
                _cameraContainer.SetPositionOffset(id, w, GetNoiseVector(s, position.offset, position.multiplier));    
                _cameraContainer.SetRotationOffset(id, w, Quaternion.Euler(GetNoiseVector(s, rotation.offset, rotation.multiplier)));    
            }
        }

        private static Vector3 GetNoiseVector(float t, Vector3 offset, Vector3 multiplier) {
            return new Vector3(
                (Mathf.PerlinNoise1D(t + offset.x) - 0.5f) * multiplier.x,
                (Mathf.PerlinNoise1D(t + offset.y) - 0.5f) * multiplier.y,
                (Mathf.PerlinNoise1D(t + offset.z) - 0.5f) * multiplier.z
            );
        }

#if UNITY_EDITOR
        private int _lastStateId;
        private void OnValidate() {
            if (!Application.isPlaying || _cameraContainer == null) return;
            
            RemoveState(_lastStateId);
            _lastStateId = CreateState(_weight);
            SetNoiseScale(_lastStateId, _noiseScale);
            SetPosition(_lastStateId, _positionOffset, _positionMultiplier);
            SetRotation(_lastStateId, _rotationOffset, _rotationMultiplier);
        }
#endif
    }
    
}