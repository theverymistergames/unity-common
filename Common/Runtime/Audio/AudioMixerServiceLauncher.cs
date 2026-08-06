using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.Audio;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class AudioMixerServiceLauncher : MonoBehaviour {
        
        [SerializeField] private AudioMixer _mixer;
        
        private readonly AudioMixerService _audioMixerService = new();
        
        private void Awake() {
            _audioMixerService.Initialize(_mixer);
            Services.Register<IAudioMixerService>(_audioMixerService);
        }

        private void OnDestroy() {
            Services.Unregister(_audioMixerService);
        }
    }
    
}