using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class AudioBankServiceLauncher : MonoBehaviour {
        
        private readonly AudioBankService _service = new();
        
        private void Awake() {
            Services.Register<IAudioBankService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
            _service.Dispose();
        }
    }
    
}