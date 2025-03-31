using System;
using MisterGames.Actors;
using MisterGames.Character.Phys;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Labels;
using MisterGames.Common.Lists;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterStepSounds : MonoBehaviour, IActorComponent {
        
        [SerializeField] private MaterialDetectorBase _materialDetector;
        [SerializeField] [Range(0f, 2f)] private float _volume = 1f;
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new Vector2(0.9f, 1.1f);
        [SerializeField] private AudioClip[] _sounds;
        [SerializeField] private MaterialSounds[] _materialSounds;
        
        [Serializable]
        private struct MaterialSounds {
            public LabelValue material;
            [Range(0f, 2f)] public float volume;
            [MinMaxSlider(0f, 2f)] public Vector2 pitch;
            public AudioClip[] clips;
        }
        
        private Transform _transform;
        private CharacterStepsPipeline _characterStepsPipeline;

        void IActorComponent.OnAwake(IActor actor) {
            _transform = transform;
            _characterStepsPipeline = actor.GetComponent<CharacterStepsPipeline>();
        }

        private void OnEnable() {
            _characterStepsPipeline.OnStep += OnStep;
        }

        private void OnDisable() {
            _characterStepsPipeline.OnStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            var clips = _sounds;
            float volume = _volume;
            float pitch = _pitch.GetRandomInRange();
            
            if (_materialDetector.TryGetMaterial(out int materialId, out _) && 
                _materialSounds.TryFind(materialId, (preset, m) => preset.material.GetValue() == m, out var materialSounds))
            {
                clips = materialSounds.clips;
                volume = materialSounds.volume;
                pitch = materialSounds.pitch.GetRandomInRange();
            }
            
            AudioPool.Main.Play(
                AudioPool.Main.ShuffleClips(clips), 
                _transform, 
                volume: volume, 
                pitch: pitch, 
                options: AudioOptions.AffectedByTimeScale
            );
        }
    }
    
}


