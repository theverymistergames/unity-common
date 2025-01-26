using MisterGames.Actors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Audio;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterStepSounds : MonoBehaviour, IActorComponent {
        
        [SerializeField] [Range(0f, 2f)] private float _volume = 1f;
        [SerializeField] [MinMaxSlider(0f, 2f)] private Vector2 _pitch = new Vector2(0.9f, 1.1f);
        [SerializeField] private AudioClip[] _sounds;

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
            AudioPool.Main.Play(AudioPool.Main.ShuffleClips(_sounds), _transform, volume: _volume, pitch: _pitch.GetRandomInRange());
        }
    }
    
}


