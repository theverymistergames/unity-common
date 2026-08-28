using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-9000)]
    public sealed class MusicServiceLauncher : MonoBehaviour {
        
        [SerializeField] private MusicServiceConfig _config;
        
        private readonly MusicService _service = new();
        
        private void Awake() {
            _service.Initialize(_config, AudioPool.Main);
            Services.Register<IMusicService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
            _service.Dispose();
        }
    }
    
}