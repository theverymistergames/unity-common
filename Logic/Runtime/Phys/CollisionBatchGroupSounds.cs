using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Easing;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(CollisionBatchGroup))]
    public sealed class CollisionBatchGroupSounds : MonoBehaviour {

        [SerializeField] private CollisionBatchGroup _collisionBatchGroup;
        
        [Header("Material Detection")]
        [SerializeField] [Min(0f)] private float _primaryMaterialWeight = 1f;
        [SerializeField] private LabelValue _defaultSurfaceMaterial;
        [SerializeField] private MaterialData[] _primaryMaterialsPerLayer;
        [SerializeField] private MaterialDetectorBase _materialDetector;
        
        [Header("Sound Settings")]
        [SerializeField] [Min(0f)] private float _soundCooldown = 0.25f;
        [SerializeField] [Min(0f)] private float _volumeMulMin = 0.1f;
        [SerializeField] [Min(0f)] private float _volumeMulMax = 1f;
        [SerializeField] private EasingType _volumeEasing = EasingType.Linear;
        [SerializeField] [Min(0f)] private float _impulseMin = 0f;
        [SerializeField] [Min(0f)] private float _impulseMax = 1f;
        [SerializeField] [Min(0f)] private float _distanceThreshold = 0.25f;
        
        [Header("Sound List")]
        [SerializeField] private MaterialSounds[] _materialSounds;
        
        [Serializable]
        private struct MaterialSounds {
            public LabelValue material;
            public LabelValue[] surfaces;
            [Range(0f, 2f)] public float volume;
            [MinMaxSlider(0f, 2f)] public Vector2 pitch;
            public AudioClip[] clips;
        }
        
        [Serializable]
        private struct MaterialData {
            public LayerMask layerMask;
            public LabelValue material;
        }

        private readonly struct RbData {
            
            public readonly float soundTime;
            public readonly Vector3 point;
            
            public RbData(float soundTime, Vector3 point) {
                this.soundTime = soundTime;
                this.point = point;
            }
        }
        
        private readonly Dictionary<(int material, int surface), int> _materialPairToIndexMap = new();
        private readonly Dictionary<int, RbData> _lastRbDataMap = new();
        private readonly Dictionary<int, int> _colliderMaterialMap = new();
        private readonly List<MaterialInfo> _materialList = new();

        private bool _hasMaterialDetector;
        
        private void Awake() {
            FetchMaterialIdToIndexMap();
            _hasMaterialDetector = _materialDetector != null;
        }

        private void OnEnable() {
            _collisionBatchGroup.OnContact += OnContact;
        }

        private void OnDisable() {
            _materialList.Clear();
            _collisionBatchGroup.OnContact -= OnContact;
        }
        
        private void OnContact(TriggerEventType evt, Rigidbody rb, int rbMaterial, Collider collider, Vector3 point, Vector3 normal, Vector3 impulse) {
            if (evt == TriggerEventType.Exit ||
                _lastRbDataMap.TryGetValue(rb.GetHashCode(), out var data) && 
                (TimeSources.scaledTime < data.soundTime + _soundCooldown || 
                 evt == TriggerEventType.Stay && (point - data.point).sqrMagnitude < _distanceThreshold * _distanceThreshold)) 
            {
                return;
            }

            if (impulse.sqrMagnitude < _impulseMin * _impulseMin) return;
            
            _lastRbDataMap[rb.GetHashCode()] = new RbData(TimeSources.scaledTime, point);
            
            float volumeMul = _impulseMax - _impulseMin > 0f
                ? Mathf.Lerp(_volumeMulMin, _volumeMulMax, _volumeEasing.Evaluate(impulse.magnitude - _impulseMin) / (_impulseMax - _impulseMin)) 
                : _volumeMulMax;
            
            var materials = GetSurfaceMaterials(collider, point, normal);

            for (int i = 0; i < materials.Count; i++) {
                var mat = materials[i];
                PlaySound(point, rbMaterial, mat.materialId, mat.weight * volumeMul);
            }
        }

        private IReadOnlyList<MaterialInfo> GetSurfaceMaterials(Collider collider, Vector3 point, Vector3 normal) {
            _materialList.Clear();
            _materialList.Add(GetSurfaceMaterial(collider));

            if (_hasMaterialDetector) {
                _materialList.AddRange(_materialDetector.GetMaterials(point, normal));   
            }
            
            float invertedWeightSum = 0f;
            for (int i = 0; i < _materialList.Count; i++) {
                invertedWeightSum += _materialList[i].weight;
            }
            
            invertedWeightSum = invertedWeightSum > 0f ? 1f / invertedWeightSum : 0f;

            for (int i = 0; i < _materialList.Count; i++) {
                var material = _materialList[i];
                _materialList[i] = new MaterialInfo(material.materialId, material.weight * invertedWeightSum);
            }

            return _materialList;
        }

        private MaterialInfo GetSurfaceMaterial(Collider collider) {
            int instanceId = collider.GetInstanceID();

            if (!_colliderMaterialMap.TryGetValue(instanceId, out int material)) {
                material = 
                    TryGetSurfaceMaterialByLayer(collider.gameObject.layer, out int materialByLayer) ? materialByLayer 
                    : collider.TryGetComponent(out SurfaceMaterial surfaceMaterial) ? surfaceMaterial.MaterialId 
                    : _defaultSurfaceMaterial.GetValue();
                
                _colliderMaterialMap[instanceId] = material;
            } 
            
            return new MaterialInfo(material, _primaryMaterialWeight);
        }

        private bool TryGetSurfaceMaterialByLayer(int layer, out int material) {
            for (int i = 0; i < _primaryMaterialsPerLayer.Length; i++) {
                ref var data = ref _primaryMaterialsPerLayer[i];
                if (!data.layerMask.Contains(layer)) continue;
                
                material = data.material.GetValue();
                return true;
            }
            
            material = 0;
            return false;
        }
        
        private void PlaySound(Vector3 point, int rbMaterial, int surfaceMaterial, float volumeMul = 1f) {
            if (!_materialPairToIndexMap.TryGetValue((rbMaterial, surfaceMaterial), out int index) &&
                !_materialPairToIndexMap.TryGetValue((0, surfaceMaterial), out index) &&
                !_materialPairToIndexMap.TryGetValue((rbMaterial, 0), out index)) 
            {
                return;
            }
            
            ref var data = ref _materialSounds[index];

            AudioPool.Main.Play(
                AudioPool.Main.ShuffleClips(data.clips), 
                point, 
                volume: data.volume * volumeMul, 
                pitch: data.pitch.GetRandomInRange(), 
                options: AudioOptions.AffectedByTimeScale | AudioOptions.ApplyOcclusion | AudioOptions.AffectedByVolumes
            );
        }
        
        private void FetchMaterialIdToIndexMap() {
            _materialPairToIndexMap.Clear();
            
            for (int i = 0; i < _materialSounds?.Length; i++) {
                ref var data = ref _materialSounds[i];
                int mat = data.material.GetValue();
                
                for (int j = 0; j < data.surfaces?.Length; j++) {
                    var surface = data.surfaces[j];
                    if (surface.IsNull()) continue;
                    
                    _materialPairToIndexMap[(mat, surface.GetValue())] = i;   
                }
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _collisionBatchGroup = GetComponent<CollisionBatchGroup>();
        }
        
        private void OnValidate() {
            if (_impulseMax < _impulseMin) _impulseMax = _impulseMin;
            if (_volumeMulMax < _volumeMulMin) _volumeMulMax = _volumeMulMin;
            
            if (!Application.isPlaying) return;

            _hasMaterialDetector = _materialDetector != null;
            FetchMaterialIdToIndexMap();
        }
#endif
    }
    
}