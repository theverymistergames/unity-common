using MisterGames.Actors;
using MisterGames.Character.Capsule;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterSwimStrokeSounds : MonoBehaviour, IActorComponent {

        [SerializeField] [Range(0f, 2f)] private float _volume = 1f;
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new Vector2(0.9f, 1.1f);
        [SerializeField] [Min(0f)] private float _playSoundCooldown = 0.3f;
        [SerializeField] private AudioClip[] _sounds;
        
        private CharacterSwimStrokePipeline _swimStrokePipeline;
        private CharacterCapsulePipeline _capsulePipeline;
        private float _nextStepSoundTime;

        void IActorComponent.OnAwake(IActor actor) {
            _swimStrokePipeline = actor.GetComponent<CharacterSwimStrokePipeline>();
            _capsulePipeline = actor.GetComponent<CharacterCapsulePipeline>();
        }

        private void OnEnable() {
            _swimStrokePipeline.OnStroke += OnStroke;
        }

        private void OnDisable() {
            _swimStrokePipeline.OnStroke -= OnStroke;
        }

        private void OnStroke(int arm, float distance, Vector3 point) {
            PlayStrokeSound(point);
        }

        private void PlayStrokeSound(Vector3 point) {
            float time = TimeSources.scaledTime;
            if (time < _nextStepSoundTime) return;
            
            _nextStepSoundTime = time + _playSoundCooldown;

            AudioPool.Main.Play(
                AudioPool.Main.ShuffleClips(_sounds), 
                point, 
                volume: _volume, 
                pitch: _pitch.GetRandomInRange(), 
                options: AudioOptions.AffectedByTimeScale | AudioOptions.AffectedByVolumes | AudioOptions.ApplyOcclusion
            );
        }
    }
    
}


