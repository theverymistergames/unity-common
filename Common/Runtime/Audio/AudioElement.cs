using System.Threading;
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
        public int MixerGroupId { get; set; }
        public IAudioPool AudioPool { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public AudioOptions AudioOptions { get; set; }
        public float PitchMul { get; set; }
        public float AttenuationMul { get; set; }
        public float ClipLength { get; set; }
        public float ClipTime { get; set; }
        public float FadeOut { get; set; }
        public int OcclusionFlag { get; set; }

        private void OnDisable() {
            AudioPool?.ReleaseAudioHandle(Id, immediate: true);
        }

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

        private void OnDrawGizmos() {
            if (!Application.isPlaying || !(AudioPool?.ShowGizmo ?? false)) return;
            
            DebugExt.DrawLabel(transform.position, $"[{Id}] {(_source.clip == null ? "<null>" : _source.clip.name)}");
        }
#endif
    }
    
}