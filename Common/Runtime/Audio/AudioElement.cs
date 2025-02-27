using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioLowPassFilter))]
    [RequireComponent(typeof(AudioHighPassFilter))]
    internal sealed class AudioElement : MonoBehaviour, IAudioElement {
        
        [SerializeField] private Transform _transform;
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioLowPassFilter _lowPass;
        [SerializeField] private AudioHighPassFilter _highPass;

        public Transform Transform => _transform;
        public AudioSource Source => _source;
        public AudioLowPassFilter LowPass => _lowPass;
        public AudioHighPassFilter HighPass => _highPass;

        public int Id { get; set; }
        public float Pitch { get; set; }
        public AudioOptions AudioOptions { get; set; }

        public override string ToString() {
            return $"{nameof(AudioElement)}({_source.clip.name})";
        }

#if UNITY_EDITOR
        private void Reset() {
            _transform = transform;
            _source = GetComponent<AudioSource>();
            _lowPass = GetComponent<AudioLowPassFilter>();
            _highPass = GetComponent<AudioHighPassFilter>();
        }  
#endif
    }
    
}