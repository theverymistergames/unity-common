using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.State {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class EnableStateServiceLauncher : MonoBehaviour {

        private readonly EnableStateService _service = new();
        
        private void Awake() {
            Services.Register<IEnableStateService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
        }
    }
    
}