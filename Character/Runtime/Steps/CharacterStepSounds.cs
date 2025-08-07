using System;
using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Capsule;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
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
        
        private CharacterStepsPipeline _stepsPipeline;
        private CharacterCapsulePipeline _capsulePipeline;
        private float _nextStepSoundTime;

        void IActorComponent.OnAwake(IActor actor) {
            _stepsPipeline = actor.GetComponent<CharacterStepsPipeline>();
            _capsulePipeline = actor.GetComponent<CharacterCapsulePipeline>();
            
            FetchMaterialIdToIndexMap();
        }

        private void OnEnable() {
            _stepsPipeline.OnStep += OnStep;
        }

        private void OnDisable() {
            _stepsPipeline.OnStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
#if UNITY_EDITOR
            if (_enableDebugSound) {
                if (_debugSound == null) return;
                
                AudioPool.Main.Play(
                    _debugSound, 
                    _capsulePipeline.Root, 
                    options: AudioOptions.AffectedByTimeScale | AudioOptions.AffectedByVolumes | AudioOptions.ApplyOcclusion
                );
                return;
            }
#endif
            
            PlayStepSound(point);
        }

        public void PlayStepSound(float volumeMul = 1f, float cooldown = -1f) {
            PlayStepSound(_capsulePipeline.GetColliderBottomPoint(_capsulePipeline.Radius), volumeMul, cooldown);
        }

        private void PlayStepSound(Vector3 stepPoint, float volumeMul = 1f, float cooldown = -1f) {
            float time = TimeSources.scaledTime;
            if (time < _nextStepSoundTime) return;
            
            _nextStepSoundTime = time + (cooldown < 0f ? _playSoundCooldown : cooldown);

            var up = _capsulePipeline.Root.up;
            var point = _capsulePipeline.GetColliderBottomPoint(_capsulePipeline.Radius);
            
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
                    stepPoint, 
                    volume: volume * info.weight * volumeMul, 
                    pitch: pitch, 
                    options: AudioOptions.AffectedByTimeScale | AudioOptions.AffectedByVolumes | AudioOptions.ApplyOcclusion
                );
            }
        }


        private void FetchMaterialIdToIndexMap() {
            _materialIdToIndexMap.Clear();
            
            for (int i = 0; i < _materialSounds?.Length; i++) {
                ref var materialSounds = ref _materialSounds[i];
                _materialIdToIndexMap[materialSounds.material.GetValue()] = i;
            }
        }

#if UNITY_EDITOR
        [SerializeField] private bool _enableDebugSound;
        [SerializeField] private AudioClip _debugSound;
        
        private void OnValidate() {
            FetchMaterialIdToIndexMap();
        }
#endif
    }
    
}


