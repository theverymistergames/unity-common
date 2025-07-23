using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterStepSounds : MonoBehaviour, IActorComponent {

        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private MaterialDetectorBase _materialDetector;
        [SerializeField] [Range(0f, 2f)] private float _volume = 1f;
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new Vector2(0.9f, 1.1f);
        [SerializeField] [Min(0f)] private float _playSoundCooldown = 0.3f;
        [SerializeField] private AudioClip[] _sounds;
        [SerializeField] private MaterialSounds[] _materialSounds;
        
        [Serializable]
        private struct MaterialSounds {
            public LabelValue material;
            [Range(0f, 2f)] public float volume;
            [MinMaxSlider(0f, 2f)] public Vector2 pitch;
            public AudioClip[] clips;
        }
        
        private readonly Dictionary<int, int> _materialIdToIndexMap = new();
        private Transform _transform;
        private CharacterStepsPipeline _characterStepsPipeline;
        private float _nextStepSoundTime;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = transform;
            _characterStepsPipeline = actor.GetComponent<CharacterStepsPipeline>();
            
            FetchMaterialIdToIndexMap();
        }

        private void OnEnable() {
            _characterStepsPipeline.OnStep += OnStep;
        }

        private void OnDisable() {
            _characterStepsPipeline.OnStep -= OnStep;
        }

        public void PlayStepSound(float volumeMul = 1f, float cooldown = -1f) {
            float time = Time.time;
            if (time < _nextStepSoundTime) return;
            
            _nextStepSoundTime = time + (cooldown < 0f ? _playSoundCooldown : cooldown);

            var up = _transform.up;
            var point = _transform.TransformPoint(_capsuleCollider.center) - _capsuleCollider.height * 0.5f * up;
            
            var materials = _materialDetector.GetMaterials(point, up);
            
            for (int i = 0; i < materials.Count; i++) {
                var info = materials[i];
                if (info.weight <= 0f) continue;
                
                var clips = _sounds;
                float volume = _volume;
                float pitch = _pitch.GetRandomInRange();

                if (_materialIdToIndexMap.TryGetValue(info.materialId, out int index)) {
                    ref var data = ref _materialSounds[index];
                    
                    clips = data.clips;
                    volume = data.volume;
                    pitch = data.pitch.GetRandomInRange();    
                }
                
                AudioPool.Main.Play(
                    AudioPool.Main.ShuffleClips(clips), 
                    _transform, 
                    volume: volume * info.weight * volumeMul, 
                    pitch: pitch, 
                    options: AudioOptions.AffectedByTimeScale | AudioOptions.AffectedByVolumes
                );   
            }
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            PlayStepSound();
        }

        private void FetchMaterialIdToIndexMap() {
            _materialIdToIndexMap.Clear();
            
            for (int i = 0; i < _materialSounds?.Length; i++) {
                ref var materialSounds = ref _materialSounds[i];
                _materialIdToIndexMap[materialSounds.material.GetValue()] = i;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            FetchMaterialIdToIndexMap();
        }
#endif
    }
    
}


