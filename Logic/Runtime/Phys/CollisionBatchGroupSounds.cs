using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(CollisionBatchGroup))]
    public sealed class CollisionBatchGroupSounds : MonoBehaviour {

        [SerializeField] private CollisionBatchGroup _collisionBatchGroup;
        
        [Header("Materials")]
        [SerializeField] [Min(0f)] private float _primaryMaterialWeight = 1f;
        [SerializeField] private MaterialData[] _primaryMaterialsPerLayer;
        [SerializeField] private MaterialDetectorBase _secondaryMaterialDetector;
        
        [Header("Sounds")]
        [SerializeField] [Min(0f)] private float _soundCooldown = 0.25f;
        [SerializeField] [Min(0f)] private float _volumeMulMin = 0.1f;
        [SerializeField] [Min(0f)] private float _volumeMulMax = 1f;
        [SerializeField] [Min(0f)] private float _impulseMax = 1f;
        [SerializeField] private MaterialSounds[] _materialSounds;
        
        [Serializable]
        private struct MaterialSounds {
            public LabelValue material;
            public LabelValue surface;
            [Range(0f, 2f)] public float volume;
            [MinMaxSlider(0f, 2f)] public Vector2 pitch;
            public AudioClip[] clips;
        }
        
        [Serializable]
        private struct MaterialData {
            public LayerMask layerMask;
            public LabelValue material;
        }
        
        private readonly Dictionary<(int material, int surface), int> _materialPairToIndexMap = new();
        private readonly Dictionary<int, float> _lastSoundTimeMap = new();
        private readonly Dictionary<int, int> _colliderMaterialMap = new();
        private readonly List<MaterialInfo> _materialList = new();

        private void OnEnable() {
            _collisionBatchGroup.OnContact += OnContact;
        }

        private void OnDisable() 
        {
            _materialList.Clear();
            _collisionBatchGroup.OnContact -= OnContact;
        }
        
        private void OnContact(TriggerEventType evt, Rigidbody rb, int rbMaterial, Collider collider, Vector3 point, Vector3 normal, Vector3 impulse) {
            if (evt != TriggerEventType.Enter ||
                _lastSoundTimeMap.TryGetValue(rb.GetInstanceID(), out float lastSoundTime) && TimeSources.scaledTime < lastSoundTime + _soundCooldown) 
            {
                return;
            }

            _lastSoundTimeMap[rb.GetInstanceID()] = TimeSources.scaledTime;

            float volumeMul = _impulseMax > 0f ? Mathf.Clamp01(impulse.sqrMagnitude / (_impulseMax * _impulseMax)) : 1f;
            var materials = GetSurfaceMaterials(collider, point, normal);

            for (int i = 0; i < materials.Count; i++) {
                var mat = materials[i];
                PlaySound(point, rbMaterial, mat.materialId, mat.weight * volumeMul);
            }
        }

        private IReadOnlyList<MaterialInfo> GetSurfaceMaterials(Collider collider, Vector3 point, Vector3 normal) {
            _materialList.Clear();
            _materialList.Add(GetSurfaceMaterial(collider));
            _materialList.AddRange(_secondaryMaterialDetector.GetMaterials(point, normal));
            
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
                    : 0;
                
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
                options: AudioOptions.AffectedByTimeScale | AudioOptions.ApplyOcclusion
            );
        }
        
        private void FetchMaterialIdToIndexMap() {
            _materialPairToIndexMap.Clear();
            
            for (int i = 0; i < _materialSounds?.Length; i++) {
                ref var data = ref _materialSounds[i];
                _materialPairToIndexMap[(data.material.GetValue(), data.surface.GetValue())] = i;
            }
        }
        
#if UNITY_EDITOR
        private void Reset() {
            _collisionBatchGroup = GetComponent<CollisionBatchGroup>();
        }
        
        private void OnValidate() {
            FetchMaterialIdToIndexMap();
        }
#endif
    }
    
}