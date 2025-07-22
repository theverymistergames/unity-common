using System;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    public sealed class SpawnPrefabOnTrigger : MonoBehaviour {
        
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private PositionMode _positionMode;
        [SerializeField] private Collider[] _colliders;
        
        [Header("Scale by Size")]
        [SerializeField] [Min(0f)] private float _minSize;
        [SerializeField] [Min(0f)] private float _maxSize = 1f;
        [SerializeField] [Min(0f)] private float _minScaleMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxScaleMul = 1f;
        
        [Header("Prefabs")]
        [SerializeField] private SpawnData[] _enterPrefabs;
        [SerializeField] private SpawnData[] _exitPrefabs;

        private enum PositionMode {
            ColliderPosition,
            ClosestPointOnBounds,
        }
        
        [Serializable]
        private struct SpawnData {
            [Min(0f)] public float minSize;
            public float scaleMul;
            public GameObject prefab;
        }
        
        private void OnEnable() {
            _triggerEmitter.TriggerEnter += TriggerEnter; 
            _triggerEmitter.TriggerExit += TriggerExit; 
        }

        private void OnDisable() {
            _triggerEmitter.TriggerEnter -= TriggerEnter; 
            _triggerEmitter.TriggerExit -= TriggerExit;
        }

        private void TriggerEnter(Collider collider) {
            float size = GetColliderSize(collider);
            int index = GetDataIndex(size, _enterPrefabs);
            if (index < 0) return;
            
            ref var data = ref _enterPrefabs[index];
            float scaleMul = GetScaleMul(size) * data.scaleMul;
            
            collider.transform.GetPositionAndRotation(out var pos, out var rot);
            
            Spawn(data.prefab, GetPosition(pos), rot, scaleMul);
        }

        private void TriggerExit(Collider collider) {
            float size = GetColliderSize(collider);
            int index = GetDataIndex(size, _exitPrefabs);
            if (index < 0) return;
            
            ref var data = ref _exitPrefabs[index];
            float scaleMul = GetScaleMul(size) * data.scaleMul;
            
            collider.transform.GetPositionAndRotation(out var pos, out var rot);
            
            Spawn(data.prefab, GetPosition(pos), rot, scaleMul);
        }

        private Vector3 GetPosition(Vector3 colliderPos) {
            switch (_positionMode) {
                case PositionMode.ColliderPosition:
                    return colliderPos;
                
                case PositionMode.ClosestPointOnBounds:
                    float minSqrDistance = float.MaxValue;
                    var point = colliderPos;
                    
                    for (int i = 0; i < _colliders.Length; i++) {
                        var c = _colliders[i];
                        var p = c.ClosestPoint(colliderPos);
                        
                        float sqrDistance = (p - colliderPos).sqrMagnitude;
                        
                        if (sqrDistance < minSqrDistance) continue;

                        point = p;
                        minSqrDistance = sqrDistance;
                    }

                    return point;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float scaleMul) {
            var instance = PrefabPool.Main.Get(prefab, position, rotation, PrefabPool.Main.ActiveSceneRoot);
            instance.transform.localScale *= scaleMul;
        }

        private static int GetDataIndex(float colliderSize, SpawnData[] sounds) {
            for (int i = 0; i < sounds.Length; i++) {
                ref var data = ref sounds[i];
                if (colliderSize > data.minSize) return i;
            }

            return -1;
        }

        private static float GetColliderSize(Collider collider) {
            var size = collider.bounds.size;
            return Mathf.Max(Mathf.Max(size.x, size.y), size.z);
        }

        private float GetScaleMul(float colliderSize) {
            float t = _maxSize - _minSize > 0f ? (colliderSize - _minSize) / (_maxSize - _minSize) : 1f;
            return Mathf.Lerp(_minScaleMul, _maxScaleMul, t);
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (_maxSize < _minSize) _maxSize = _minSize;
            if (_maxScaleMul < _minScaleMul) _maxScaleMul = _minScaleMul;

            ValidateSoundData(_enterPrefabs);
            ValidateSoundData(_exitPrefabs);
        }

        private static void ValidateSoundData(SpawnData[] sounds) {
            float size = 0f;
            
            for (int i = 0; i < sounds?.Length; i++) {
                ref var data = ref sounds[i];
                if (data.minSize < size) data.minSize = size;
                size = data.minSize;
            }
        }
#endif
    }
    
}