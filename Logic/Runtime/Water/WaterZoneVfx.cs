using System;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Logic.Water {
    
    [RequireComponent(typeof(WaterZone))]
    public sealed class WaterZoneVfx : MonoBehaviour {
        
        [SerializeField] private WaterZone _waterZone;
        
        [Header("Scale by Size")]
        [SerializeField] [Min(0f)] private float _minSize;
        [SerializeField] [Min(0f)] private float _maxSize = 1f;
        [SerializeField] [Min(0f)] private float _minSizeScaleMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxSizeScaleMul = 1f;
        
        [Header("Scale by Speed")]
        [SerializeField] [Min(0f)] private float _minSpeed;
        [SerializeField] [Min(0f)] private float _maxSpeed = 1f;
        [SerializeField] [Min(0f)] private float _minSpeedScaleMul = 0.5f;
        [SerializeField] [Min(0f)] private float _maxSpeedScaleMul = 1f;
        
        [Header("Prefabs")]
        [SerializeField] private SpawnData[] _enterPrefabs;
        [SerializeField] private SpawnData[] _exitPrefabs;
        
        [Serializable]
        private struct SpawnData {
            [Min(0f)] public float minColliderSize;
            [Min(0f)] public float minSpeed;
            public float scaleMul;
            public GameObject prefab;
        }
        
        private void OnEnable() {
            _waterZone.OnColliderEnter += OnColliderEnter;
            _waterZone.OnColliderExit += OnColliderExit;
        }

        private void OnDisable() {
            _waterZone.OnColliderEnter -= OnColliderEnter;
            _waterZone.OnColliderExit -= OnColliderExit;
        }

        private void OnColliderEnter(Collider collider, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal) {
            float size = GetColliderSize(collider);
            float sqrSpeed = GetSqrSpeed(collider);
            
            int index = GetDataIndex(size, sqrSpeed, _enterPrefabs);
            if (index < 0) return;
            
            ref var data = ref _enterPrefabs[index];
            float scaleMul = GetScaleMul(size, sqrSpeed) * data.scaleMul;

            var rot = Quaternion.LookRotation(RandomExtensions.OnUnitCircle(surfaceNormal), surfaceNormal);
            
            Spawn(data.prefab, position, rot, scaleMul);
        }
        
        private void OnColliderExit(Collider collider, Vector3 position, Vector3 surfacePoint, Vector3 surfaceNormal) {
            if (collider == null) return;
            
            float size = GetColliderSize(collider);
            float sqrSpeed = GetSqrSpeed(collider);
            
            int index = GetDataIndex(size, sqrSpeed, _exitPrefabs);
            if (index < 0) return;
            
            ref var data = ref _exitPrefabs[index];
            float scaleMul = GetScaleMul(size, sqrSpeed) * data.scaleMul;
            
            var rot = Quaternion.LookRotation(RandomExtensions.OnUnitCircle(surfaceNormal), surfaceNormal);
            
            Spawn(data.prefab, position, rot, scaleMul);
        }
        
        private static void Spawn(GameObject prefab, Vector3 position, Quaternion rotation, float scaleMul) {
            var instance = PrefabPool.Main.Get(prefab, position, rotation, PrefabPool.Main.ActiveSceneRoot);
            instance.transform.localScale *= scaleMul;
        }

        private static int GetDataIndex(float colliderSize, float sqrSpeed, SpawnData[] prefabs) {
            if (sqrSpeed < 0f) return -1;
            
            for (int i = prefabs.Length - 1; i >= 0; i--) {
                ref var data = ref prefabs[i];
                
                if (colliderSize >= data.minColliderSize && sqrSpeed >= data.minSpeed * data.minSpeed) {
                    return i;
                }
            }

            return -1;
        }

        private static float GetColliderSize(Collider collider) {
            var size = collider.bounds.size;
            return Mathf.Max(Mathf.Max(size.x, size.y), size.z);
        }

        private static float GetSqrSpeed(Collider collider) {
            if (collider.attachedRigidbody is not { } rb || rb.isKinematic) return -1f;
            
            return rb.linearVelocity.sqrMagnitude;
        }
        
        private float GetScaleMul(float colliderSize, float sqrSpeed) {
            float tSize = _maxSize - _minSize > 0f ? (colliderSize - _minSize) / (_maxSize - _minSize) : 1f;
            float tSpeed = sqrSpeed >= 0f && _maxSpeed - _minSpeed > 0f ? (Mathf.Sqrt(sqrSpeed) - _minSpeed) / (_maxSpeed - _minSpeed) : 1f;
            
            return Mathf.Lerp(_minSizeScaleMul, _maxSizeScaleMul, tSize) * Mathf.Lerp(_minSpeedScaleMul, _maxSpeedScaleMul, tSpeed);
        }

#if UNITY_EDITOR
        private void Reset() {
            _waterZone = GetComponent<WaterZone>();
        }
        
        private void OnValidate() {
            if (_maxSize < _minSize) _maxSize = _minSize;
            if (_maxSizeScaleMul < _minSizeScaleMul) _maxSizeScaleMul = _minSizeScaleMul;

            ValidateSoundData(_enterPrefabs);
            ValidateSoundData(_exitPrefabs);
        }

        private static void ValidateSoundData(SpawnData[] sounds) {
            float size = 0f;
            
            for (int i = 0; i < sounds?.Length; i++) {
                ref var data = ref sounds[i];
                if (data.minColliderSize < size) data.minColliderSize = size;
                size = data.minColliderSize;
            }
        }
#endif
    }
    
}