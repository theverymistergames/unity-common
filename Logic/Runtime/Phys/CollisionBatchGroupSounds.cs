using System;
using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(CollisionBatchGroup))]
    public sealed class CollisionBatchGroupSounds : MonoBehaviour {

        [SerializeField] private CollisionBatchGroup _collisionBatchGroup;
        [SerializeField] [Min(0f)] private float _soundCooldown = 0.25f;
        [SerializeField] [Min(0f)] private float _volumeMulMin = 0.1f;
        [SerializeField] [Min(0f)] private float _volumeMulMax = 1f;
        [SerializeField] [Min(0f)] private float _impulseMax = 1f;
        [SerializeField] private MaterialSounds[] _materialSounds;
        
        [Serializable]
        private struct MaterialSounds {
            public LabelValue material;
            [Range(0f, 2f)] public float volume;
            [MinMaxSlider(0f, 2f)] public Vector2 pitch;
            public AudioClip[] clips;
        }
        
        private readonly Dictionary<int, int> _materialIdToIndexMap = new();
        private readonly Dictionary<int, float> _lastSoundTimeMap = new();
        
        private void OnEnable() {
            _collisionBatchGroup.OnContact += OnContact;
        }

        private void OnDisable() 
        {
            _collisionBatchGroup.OnContact -= OnContact;
        }

        private void OnContact(TriggerEventType evt, Rigidbody rb, int surfaceMaterial, Vector3 point, Vector3 normal, Vector3 impulse) {
            if (evt != TriggerEventType.Enter ||
                _lastSoundTimeMap.TryGetValue(rb.GetInstanceID(), out float lastSoundTime) && TimeSources.scaledTime < lastSoundTime + _soundCooldown) 
            {
                return;
            }
            
            _lastSoundTimeMap[rb.GetInstanceID()] = TimeSources.scaledTime;

            float volumeMul = _impulseMax > 0f ? Mathf.Clamp01(impulse.sqrMagnitude / (_impulseMax * _impulseMax)) : 1f;
            PlaySound(point, surfaceMaterial, volumeMul);
        }
        
        private void PlaySound(Vector3 point, int surfaceMaterial, float volumeMul = 1f) {
            if (!_materialIdToIndexMap.TryGetValue(surfaceMaterial, out int index)) return;
            
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
            _materialIdToIndexMap.Clear();
            
            for (int i = 0; i < _materialSounds?.Length; i++) {
                ref var materialSounds = ref _materialSounds[i];
                _materialIdToIndexMap[materialSounds.material.GetValue()] = i;
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