using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Scenes.SceneRoots {
    
    [DefaultExecutionOrder(-200_000)]
    public sealed class SceneRootServiceRunner : MonoBehaviour {
        
        private readonly SceneRootService _service = new();

        private void Awake() {
            _service.Initialize();
            Services.Register<ISceneRootService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
            _service.Dispose();
        }
    }
    
}