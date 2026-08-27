using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class GameplaySettingsServiceLauncher : MonoBehaviour {
        
        private readonly GameplayRuntimeSettings _gameplayRuntimeSettings = new();
        
        private void Awake() {
            Services.Register<IGameplayRuntimeSettings>(_gameplayRuntimeSettings);
        }

        private void OnDestroy() {
            Services.Unregister(_gameplayRuntimeSettings);
        }
    }
    
}