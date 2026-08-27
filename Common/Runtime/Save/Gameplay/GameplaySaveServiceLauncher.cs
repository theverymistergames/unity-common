using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Save {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class GameplaySaveServiceLauncher : MonoBehaviour {

        [SerializeField] private GameplaySaveSettings _gameplaySaveSettings;

        private readonly GameplaySaveService _gameplaySaveService = new();
        
        private void Awake() {
            _gameplaySaveService.Initialize(_gameplaySaveSettings, SaveSystem.Main);
            Services.Register<IGameplaySaveService>(_gameplaySaveService);
        }

        private void OnDestroy() {
            Services.Unregister(_gameplaySaveService);
            _gameplaySaveService.Dispose();
        }
    }
    
}