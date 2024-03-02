using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Character.Steps {
    
    public sealed class CharacterStepSounds : MonoBehaviour {

        [SerializeField] private CharacterStepsPipeline _characterStepsPipeline;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _sounds;

        private void OnEnable() {
            _characterStepsPipeline.OnStep += OnStep;
        }

        private void OnDisable() {
            _characterStepsPipeline.OnStep -= OnStep;
        }

        private void OnStep(int foot, float distance, Vector3 point) {
            _audioSource.PlayOneShot(_sounds.GetRandom());
        }
    }
    
}


