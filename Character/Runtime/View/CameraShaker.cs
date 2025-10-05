using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CameraShaker : MonoBehaviour, IActorComponent, IUpdate {

        [Header("Test")]
        [SerializeField] private float _weight;
        [SerializeField] private Vector3 _speed;
        [SerializeField] private Vector3 _positionOffset;
        [SerializeField] private Vector3 _positionMultiplier;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] private Vector3 _rotationMultiplier;
        
        private readonly Dictionary<int, float> _weightMap = new();
        private readonly Dictionary<int, float> _timeMap = new();
        private readonly Dictionary<int, VectorData> _speedMap = new();
        private readonly Dictionary<int, VectorData> _positionMap = new();
        private readonly Dictionary<int, VectorData> _rotationMap = new();
        
        private CameraContainer _cameraContainer;

        private readonly struct VectorData {
            
            public readonly Vector3 offset;
            public readonly Vector3 multiplier;
            
            public VectorData(Vector3 offset, Vector3 multiplier) {
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
            _timeMap[id] = TimeSources.scaledTime;
            _speedMap[id] = default;
            _positionMap[id] = default;
            _rotationMap[id] = default;
            
            PlayerLoopStage.LateUpdate.Subscribe(this);
            
            return id;
        }

        public void RemoveState(int id) {
            _cameraContainer.RemoveState(id);
            
            _weightMap.Remove(id);
            _timeMap.Remove(id);
            _speedMap.Remove(id);
            _positionMap.Remove(id);
            _rotationMap.Remove(id);
            
            if (_weightMap.Count == 0) PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        public void SetWeight(int id, float weight) {
            _weightMap[id] = weight;
        }

        public void SetSpeed(int id, Vector3 speed) {
            float time = TimeSources.scaledTime;
            var lastSpeed = _speedMap.GetValueOrDefault(id);
            var offset = lastSpeed.offset + lastSpeed.multiplier * (time - _timeMap[id]);
            
            _speedMap[id] = new VectorData(offset, speed);
            _timeMap[id] = time;
        }

        public void SetPosition(int id, Vector3 offset, Vector3 multiplier) {
            _positionMap[id] = new VectorData(offset, multiplier);
        }
        
        public void SetRotation(int id, Vector3 offset, Vector3 multiplier) {
            _rotationMap[id] = new VectorData(offset, multiplier);
        }

        void IUpdate.OnUpdate(float dt) {
            float time = TimeSources.scaledTime;
            
            foreach ((int id, float w) in _weightMap) {
                var position = _positionMap[id];
                var rotation = _rotationMap[id];
                var speed = _speedMap[id];
                var t = (time - _timeMap[id]) * speed.multiplier + speed.offset;

                _cameraContainer.SetPositionOffset(id, w, GetNoiseVector(t + position.offset, position.multiplier));    
                _cameraContainer.SetRotationOffset(id, w, Quaternion.Euler(GetNoiseVector(t + rotation.offset, rotation.multiplier)));    
            }
        }

        private static Vector3 GetNoiseVector(Vector3 t, Vector3 multiplier) {
            return new Vector3(
                (Mathf.PerlinNoise1D(t.x) - 0.5f) * multiplier.x,
                (Mathf.PerlinNoise1D(t.y) - 0.5f) * multiplier.y,
                (Mathf.PerlinNoise1D(t.z) - 0.5f) * multiplier.z
            );
        }

#if UNITY_EDITOR
        private int _lastStateId;
        private void OnValidate() {
            if (!Application.isPlaying || _cameraContainer == null) return;
            
            RemoveState(_lastStateId);
            _lastStateId = CreateState(_weight);
            
            SetSpeed(_lastStateId, _speed);
            SetPosition(_lastStateId, _positionOffset, _positionMultiplier);
            SetRotation(_lastStateId, _rotationOffset, _rotationMultiplier);
        }
#endif
    }
    
}