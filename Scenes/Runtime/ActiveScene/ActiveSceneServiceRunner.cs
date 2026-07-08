using MisterGames.Common.Service;
using MisterGames.Scenes.SceneRoots;
using UnityEngine;

namespace MisterGames.Scenes.ActiveScene {
    
    [DefaultExecutionOrder(-90_000)]
    public sealed class ActiveSceneServiceLauncher : MonoBehaviour {
        
        [SerializeField] private ActiveSceneSettings _activeSceneSettings;
        
        private readonly ActiveSceneService _service = new();
        
        private void Awake() {
            _service.Initialize(_activeSceneSettings, Services.Get<ISceneRootService>());
        }

        private void OnDestroy() {
            _service.Dispose();
        }
    }
    
}